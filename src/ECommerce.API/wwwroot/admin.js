const workspace = document.querySelector('#admin-workspace');
const statusLine = document.querySelector('#admin-status');
const editor = document.querySelector('#product-editor');
const lookup = document.querySelector('#lookup');
const catalog = '/api/admin/catalog';
const session = JSON.parse(sessionStorage.getItem('riwaq-session') || 'null');
let editing = null;
let catalogPage = 1;

async function request(path, method = 'GET', body) {
  const headers = { Authorization: `Bearer ${session.token}` };
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  const response = await fetch(path, { method, headers, body: JSON.stringify(body) });
  const data = response.headers.get('content-type')?.includes('json') ? await response.json() : null;
  if (response.status === 401) throw new Error('ارجع للمتجر وسجّل الدخول بحساب الأدمن.');
  if (response.status === 403) throw new Error('الحساب ده معندوش صلاحية إدارة المنتجات.');
  if (response.status === 409) throw new Error(data?.title || 'المنتج اتغيّر أو البيانات مكررة. حمّله تاني قبل التعديل.');
  if (!response.ok) throw new Error(data?.title || `تعذّر إتمام الطلب (${response.status}).`);
  return data;
}

async function act(control, action) {
  if (control) control.disabled = true;
  statusLine.textContent = '';
  try { await action(); }
  catch (error) { statusLine.textContent = error.message; }
  finally { if (control) control.disabled = false; }
}

function setEditor(product) {
  editing = product;
  editor.reset();
  document.querySelector('#editor-title').textContent = product ? `تعديل المنتج #${product.id}` : 'إضافة منتج';
  if (!product) return;
  lookup.elements.id.value = product.id;
  for (const [name, value] of Object.entries(product)) {
    if (editor.elements.namedItem(name)) editor.elements.namedItem(name).value = value ?? '';
  }
}

async function loadProduct(id) {
  setEditor(await request(`${catalog}/products/${id}`));
  editor.elements.name.focus();
}

async function listProducts(page) {
  const result = await request(`/api/products?pageSize=12&pageNumber=${page}`);
  catalogPage = page;
  const rows = result.items.map((product) => {
    const row = document.createElement('div');
    row.className = 'row';
    const label = document.createElement('span');
    label.textContent = `${product.name} · #${product.id}`;
    const button = document.createElement('button');
    button.className = 'button outline';
    button.textContent = 'تعديل';
    button.addEventListener('click', () => act(button, () => loadProduct(product.id)));
    row.append(label, button);
    return row;
  });
  document.querySelector('#admin-products').replaceChildren(...rows);
  document.querySelector('#admin-back').disabled = page <= 1;
  document.querySelector('#admin-next').disabled = page * result.pageSize >= result.totalCount;
}

lookup.addEventListener('submit', (event) => {
  event.preventDefault();
  act(event.submitter, () => loadProduct(lookup.elements.id.value));
});
document.querySelector('#new-product').addEventListener('click', () => setEditor(null));
document.querySelector('#admin-back').addEventListener('click', () => act(null, () => listProducts(catalogPage - 1)));
document.querySelector('#admin-next').addEventListener('click', () => act(null, () => listProducts(catalogPage + 1)));
editor.addEventListener('submit', (event) => {
  event.preventDefault();
  act(event.submitter, async () => {
    const data = Object.fromEntries(new FormData(editor));
    data.categoryId = Number(data.categoryId);
    data.brandId = data.brandId ? Number(data.brandId) : null;
    data.price = Number(data.price);
    data.stockQuantity = Number(data.stockQuantity);
    data.isActive = data.isActive === 'true';
    if (editing) data.version = editing.version;
    const saved = editing
      ? await request(`${catalog}/products/${editing.id}`, 'PUT', data)
      : await request(`${catalog}/products`, 'POST', data);
    setEditor(saved);
    await listProducts(1);
    statusLine.textContent = `تم حفظ المنتج #${saved.id}.`;
  });
});

document.querySelector('#taxonomy-editor').addEventListener('submit', (event) => {
  event.preventDefault();
  const taxonomy = event.currentTarget;
  act(event.submitter, async () => {
    const { kind, name, slug } = Object.fromEntries(new FormData(taxonomy));
    const saved = await request(`${catalog}/${kind}`, 'POST', { name, slug });
    const field = kind === 'categories' ? 'categoryId' : 'brandId';
    editor.elements[field].add(new Option(saved.name, saved.id));
    editor.elements[field].value = saved.id;
    taxonomy.reset();
    statusLine.textContent = `تمت إضافة ${saved.name}.`;
  });
});

act(null, async () => {
  if (!session?.token) throw new Error('سجّل الدخول من المتجر بحساب الأدمن.');
  await request(`${catalog}/access`);
  const [categories, brands] = await Promise.all([request('/api/categories'), request('/api/brands')]);
  editor.elements.categoryId.add(new Option('اختار التصنيف', ''));
  for (const category of categories) editor.elements.categoryId.add(new Option(category.name, category.id));
  editor.elements.brandId.add(new Option('بدون علامة تجارية', ''));
  for (const brand of brands) editor.elements.brandId.add(new Option(brand.name, brand.id));
  await listProducts(1);
  workspace.hidden = false;
});
