const $ = (selector) => document.querySelector(selector);
const filters = $('#filters');
const panel = $('#panel');
const content = $('#panel-content');
const currency = new Intl.NumberFormat('ar-EG', { style: 'currency', currency: 'EGP' });
const messages = {
  'The simulated payment was declined.': 'الدفع التجريبي اترفض. تقدر تحاول تاني.',
  'The coupon is invalid or expired.': 'كود الخصم غير صالح أو منتهي.',
  'The requested quantity is unavailable.': 'الكمية المطلوبة مش متاحة.',
  'Invalid email or password.': 'البريد الإلكتروني أو كلمة المرور غير صحيحة.',
  'This email is already registered.': 'البريد الإلكتروني ده مسجّل بالفعل.'
};
const demoArt = {
  'studio-headphones': 'headphones',
  'compact-keyboard': 'keyboard',
  'arc-wireless-mouse': 'mouse',
  'desk-light': 'light',
  'everyday-usb-c-hub': 'hub',
  'felt-desk-mat': 'mat'
};
let session = JSON.parse(sessionStorage.getItem('riwaq-session') || 'null');
let page = 1;
let catalogRequest = 0;
let panelRequest = 0;
let noticeTimer;

function element(tag, value, className) {
  const node = document.createElement(tag);
  if (value != null) node.textContent = value;
  if (className) node.className = className;
  return node;
}

function button(label, action, className = 'button outline') {
  const node = element('button', label, className);
  node.type = 'button';
  node.addEventListener('click', () => run(action, node));
  return node;
}

async function run(action, control) {
  if (control) control.disabled = true;
  try { await action(); }
  catch (error) { notify(error.message); }
  finally { if (control) control.disabled = false; }
}

async function api(path, { method = 'GET', body } = {}) {
  const headers = {};
  if (session?.token) headers.Authorization = `Bearer ${session.token}`;
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  const response = await fetch(path, { method, headers, body: body === undefined ? undefined : JSON.stringify(body) });
  const data = response.headers.get('Content-Type')?.includes('json') ? await response.json() : null;
  if (!response.ok) {
    if (response.status === 401 && !path.startsWith('/api/auth/')) {
      setSession(null);
      throw new Error('جلسة الدخول انتهت. سجّل الدخول مرة تانية.');
    }
    const detail = Object.values(data?.errors || {}).flat()[0];
    const message = detail || data?.message || data?.title;
    throw Object.assign(new Error(messages[message] || message || `تعذّر إتمام الطلب (${response.status}).`), { status: response.status });
  }
  return data;
}

function notify(message) {
  const notice = $('#notice');
  notice.textContent = message;
  notice.hidden = false;
  clearTimeout(noticeTimer);
  noticeTimer = setTimeout(() => { notice.hidden = true; }, 4200);
}

function setSession(value) {
  session = value;
  if (value) sessionStorage.setItem('riwaq-session', JSON.stringify(value));
  else sessionStorage.removeItem('riwaq-session');
  $('#bag-count').textContent = '0';
}

function cartKey() { return `riwaq-cart:${session?.email || 'guest'}`; }

async function currentCart(create = false) {
  const key = cartKey();
  const savedId = sessionStorage.getItem(key);
  if (savedId) {
    try {
      const cart = await api(`/api/carts/${savedId}`);
      if (!create || !cart.checkedOut) return cart;
    } catch (error) {
      if (error.status !== 404) throw error;
      sessionStorage.removeItem(key);
    }
  }
  if (!create) return null;
  const cart = await api('/api/carts', { method: 'POST' });
  sessionStorage.setItem(key, cart.id);
  return cart;
}

function updateBadge(cart) {
  $('#bag-count').textContent = cart && !cart.checkedOut ? cart.items.reduce((sum, item) => sum + item.quantity, 0) : 0;
}

let adding = Promise.resolve();
function addProduct(productId) {
  const add = adding.then(async () => {
    const cart = await currentCart(true);
    const quantity = (cart.items.find((item) => item.productId === productId)?.quantity || 0) + 1;
    updateBadge(await setQuantity(cart.id, productId, quantity));
    notify('اتضاف المنتج للسلة.');
  });
  adding = add.catch(() => {});
  return add;
}

function setQuantity(cartId, productId, quantity) {
  return api(`/api/carts/${cartId}/items/${productId}`, { method: 'PUT', body: { quantity } });
}

async function loadFilters() {
  const lists = await Promise.all([api('/api/categories'), api('/api/brands')]);
  for (const [index, name] of ['categoryId', 'brandId'].entries()) {
    for (const item of lists[index]) filters.elements[name].add(new Option(item.name, item.id));
  }
}

function productCard(product) {
  const article = element('article', null, 'product');
  const visual = button('⌘', () => showProduct(product.id), 'product-art');
  visual.setAttribute('aria-label', `عرض ${product.name}`);
  const art = Object.hasOwn(demoArt, product.slug) ? demoArt[product.slug] : null;
  if (art) {
    visual.classList.add('demo-art', `art-${art}`);
    visual.textContent = '';
  } else if (product.brandName) {
    visual.textContent = product.brandName.trim().charAt(0);
  }
  const body = element('div', null, 'product-body');
  const title = element('h3');
  title.append(button(product.name, () => showProduct(product.id), 'title-button'));
  const bottom = element('div', null, 'product-bottom');
  const add = product.inStock ? button('أضف للسلة', () => addProduct(product.id), 'button primary') : element('span', 'غير متاح', 'muted');
  bottom.append(element('span', currency.format(product.price), 'price'), add);
  body.append(element('p', [product.categoryName, product.brandName].filter(Boolean).join(' · ')), title, bottom);
  article.append(visual, body);
  return article;
}

async function loadProducts() {
  const request = ++catalogRequest;
  const data = new FormData(filters);
  const [sortBy, sortOrder] = data.get('sort').split(':');
  const params = new URLSearchParams({ pageNumber: page, pageSize: 9, sortBy, sortOrder });
  for (const key of ['search', 'categoryId', 'brandId']) if (data.get(key)) params.set(key, data.get(key));
  try {
    const result = await api(`/api/products?${params}`);
    if (request !== catalogRequest) return;
    $('#result-count').textContent = `${result.totalCount} منتج`;
    $('#products').replaceChildren(...(result.items.length ? result.items.map(productCard) : [element('p', 'مفيش منتجات مطابقة للبحث.', 'empty')]));
    const pages = Math.ceil(result.totalCount / result.pageSize);
    $('#pagination').hidden = pages <= 1;
    $('#previous').disabled = page === 1;
    $('#next').disabled = page >= pages;
    $('#page-number').textContent = `${page} / ${pages}`;
  } catch (error) {
    if (request === catalogRequest) {
      $('#products').replaceChildren(element('p', 'تعذّر تحميل الكتالوج. تأكد إن قاعدة البيانات شغّالة وحاول تاني.', 'empty'));
      $('#pagination').hidden = true;
    }
  }
}

function openPanel(title) {
  $('#panel-title').textContent = title;
  content.replaceChildren(element('p', 'جارٍ التحميل…', 'muted'));
  if (!panel.open) panel.showModal();
  return ++panelRequest;
}

function showPanel(request, body) {
  if (request === panelRequest && panel.open) content.replaceChildren(body);
}

function field(label, name, { type = 'text', required = true, value, autocomplete } = {}) {
  const wrapper = element('label', label, 'field');
  const input = document.createElement('input');
  Object.assign(input, { type, name, required });
  if (value != null) input.value = value;
  if (autocomplete) input.autocomplete = autocomplete;
  wrapper.append(input);
  return wrapper;
}

function form(submitLabel, action, ...fields) {
  const node = element('form', null, 'stack');
  const submit = element('button', submitLabel, 'button primary');
  submit.type = 'submit';
  const error = element('p', null, 'form-error');
  error.setAttribute('role', 'alert');
  error.hidden = true;
  node.append(...fields, submit, error);
  node.addEventListener('submit', async (event) => {
    event.preventDefault();
    submit.disabled = true;
    error.hidden = true;
    try { await action(Object.fromEntries(new FormData(node))); }
    catch (failure) { error.textContent = failure.message; error.hidden = false; }
    finally { submit.disabled = false; }
  });
  return node;
}

async function showProduct(id) {
  const request = openPanel('تفاصيل المنتج');
  const [product, reviews] = await Promise.all([api(`/api/products/${id}`), api(`/api/products/${id}/reviews`)]);
  const body = element('div', null, 'stack');
  if (Object.hasOwn(demoArt, product.slug)) {
    const photo = element('div', null, `product-art demo-art art-${demoArt[product.slug]}`);
    photo.setAttribute('role', 'img');
    photo.setAttribute('aria-label', product.name);
    body.append(photo);
  }
  body.append(element('h3', product.name), element('p', product.description || product.categoryName, 'muted'), element('p', currency.format(product.price), 'price'));
  if (product.inStock) body.append(button('أضف للسلة', () => addProduct(id), 'button primary'));
  body.append(element('h3', 'المراجعات'));
  if (!reviews.items.length) body.append(element('p', 'لسه مفيش مراجعات للمنتج.', 'muted'));
  for (const review of reviews.items) {
    const entry = element('div', null, 'review');
    entry.append(element('strong', `★ ${review.rating} / 5`), element('p', review.comment || ''));
    body.append(entry);
  }
  if (session) {
    body.append(form('احفظ المراجعة', async (data) => {
      await api(`/api/products/${id}/reviews/me`, { method: 'PUT', body: { rating: Number(data.rating), comment: data.comment } });
      await showProduct(id);
    }, field('التقييم من 1 إلى 5', 'rating', { type: 'number', value: 5 }), field('تعليق', 'comment', { required: false })));
    const rating = body.querySelector('[name="rating"]');
    rating.min = 1;
    rating.max = 5;
  }
  showPanel(request, body);
}

async function showAccount(register = false) {
  const request = openPanel(session ? 'حسابي' : register ? 'إنشاء حساب' : 'تسجيل الدخول');
  const body = element('div', null, 'stack');
  if (session) {
    body.append(element('p', session.email), button('طلباتي', showOrders), button('عناويني', showAddresses), button('تسجيل الخروج', async () => {
      setSession(null);
      updateBadge(await currentCart());
      await showAccount();
    }));
    try {
      await api('/api/admin/catalog/access');
      const adminLink = element('a', 'إدارة المنتجات', 'button outline');
      adminLink.href = '/admin.html';
      body.append(adminLink);
    } catch (error) {
      if (error.status !== 403) throw error;
    }
  } else {
    body.append(form(register ? 'إنشاء الحساب' : 'دخول', async (data) => {
      if (register) await api('/api/auth/register', { method: 'POST', body: data });
      const login = await api('/api/auth/login', { method: 'POST', body: data });
      setSession({ email: data.email.trim().toLowerCase(), token: login.accessToken });
      updateBadge(await currentCart());
      await showAccount();
    }, field('البريد الإلكتروني', 'email', { type: 'email', autocomplete: 'username' }), field('كلمة المرور', 'password', { type: 'password', autocomplete: register ? 'new-password' : 'current-password' })));
    body.append(button(register ? 'عندي حساب' : 'إنشاء حساب جديد', () => showAccount(!register)));
  }
  showPanel(request, body);
}

function addressForm(onSave, address) {
  const fields = [['اسم المستلم', 'recipientName'], ['العنوان', 'line1'], ['تفاصيل إضافية', 'line2'], ['المدينة', 'city'], ['المحافظة', 'region'], ['الرمز البريدي', 'postalCode'], ['رمز البلد', 'countryCode']];
  return form('احفظ العنوان', async (data) => {
    await api(address ? `/api/addresses/${address.id}` : '/api/addresses', { method: address ? 'PUT' : 'POST', body: data });
    await onSave();
  }, ...fields.map(([label, key]) => field(label, key, { required: key !== 'line2', value: address?.[key] ?? (key === 'countryCode' ? 'EG' : '') })));
}

async function showAddresses() {
  const request = openPanel('عناويني');
  const addresses = await api('/api/addresses');
  const body = element('div', null, 'stack');
  for (const address of addresses) {
    const row = element('div', null, 'row');
    row.append(element('p', `${address.line1}، ${address.city}`), button('تعديل', () => {
      const edit = openPanel('تعديل العنوان');
      showPanel(edit, addressForm(showAddresses, address));
    }), button('حذف', async () => {
      await api(`/api/addresses/${address.id}`, { method: 'DELETE' });
      await showAddresses();
    }));
    body.append(row);
  }
  body.append(element('h3', 'عنوان جديد'), addressForm(showAddresses));
  showPanel(request, body);
}

async function showCart() {
  const request = openPanel('سلة التسوّق');
  const cart = await currentCart();
  updateBadge(cart);
  const body = element('div', null, 'stack');
  if (cart?.checkedOut) {
    body.append(button('عرض آخر طلب', async () => showOrder(await api(`/api/carts/${cart.id}/order`))));
  } else if (!cart?.items.length) {
    body.append(element('p', 'السلة فاضية.', 'empty'));
  } else {
    for (const item of cart.items) {
      const row = element('div', null, 'row');
      const qty = element('div', null, 'qty');
      const adjust = (quantity) => async () => { await setQuantity(cart.id, item.productId, quantity); await showCart(); };
      const minus = button('−', adjust(item.quantity - 1), '');
      minus.setAttribute('aria-label', `قلل كمية ${item.name}`);
      const plus = button('+', adjust(item.quantity + 1), '');
      plus.setAttribute('aria-label', `زوّد كمية ${item.name}`);
      qty.append(minus, element('span', item.quantity), plus);
      row.append(element('strong', item.name), element('span', currency.format(item.unitPrice * item.quantity)), qty);
      body.append(row);
    }
    body.append(element('p', `المجموع ${currency.format(cart.items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0))}`, 'price'), button('إتمام الطلب', () => showCheckout(cart.id), 'button primary'));
  }
  showPanel(request, body);
}

async function showCheckout(cartId) {
  const request = openPanel('إتمام الطلب');
  const body = element('div', null, 'stack');
  body.append(element('p', 'الدفع تجريبي. اختار قبول أو رفض لمحاكاة النتيجة.', 'muted'));
  const fields = [];
  const addresses = session ? await api('/api/addresses') : [];
  if (session && !addresses.length) {
    body.append(element('h3', 'أضف عنوان الشحن'), addressForm(() => showCheckout(cartId)));
    showPanel(request, body);
    return;
  }
  if (session) {
    const label = element('label', 'عنوان الشحن', 'field');
    const select = element('select');
    select.name = 'addressId';
    for (const address of addresses) select.add(new Option(`${address.line1}، ${address.city}`, address.id));
    label.append(select);
    fields.push(label);
  }
  const payment = element('label', 'نتيجة الدفع التجريبي', 'field');
  const select = element('select');
  select.name = 'approvePayment';
  select.add(new Option('قبول الدفع', 'true'));
  select.add(new Option('رفض الدفع', 'false'));
  payment.append(select);
  body.append(form('جرّب الدفع', async (data) => {
    const order = await api(`/api/carts/${cartId}/demo-checkout`, { method: 'POST', body: { addressId: data.addressId || null, couponCode: data.couponCode.trim() || null, approvePayment: data.approvePayment === 'true' } });
    updateBadge(null);
    await loadProducts();
    showOrder(order);
  }, ...fields, field('كود الخصم', 'couponCode', { required: false }), payment));
  showPanel(request, body);
}

function showOrder(order) {
  const request = openPanel('تفاصيل الطلب');
  const body = element('div', null, 'stack');
  body.append(element('h3', 'الطلب اتسجّل بنجاح'), element('p', `مرجع الدفع التجريبي ${order.demoPaymentReference}`, 'muted'));
  for (const item of order.items) body.append(element('p', `${item.name} × ${item.quantity}`));
  if (order.discount) body.append(element('p', `الخصم ${currency.format(order.discount)}`));
  body.append(element('p', `الإجمالي ${currency.format(order.total)}`, 'price'));
  showPanel(request, body);
}

async function showOrders() {
  const request = openPanel('طلباتي');
  const orders = await api('/api/orders');
  const body = element('div', null, 'stack');
  if (!orders.length) body.append(element('p', 'لسه مفيش طلبات.', 'empty'));
  for (const order of orders) body.append(button(`${new Date(order.createdAt).toLocaleDateString('ar-EG')} · ${currency.format(order.total)}`, () => showOrder(order)));
  showPanel(request, body);
}

filters.addEventListener('submit', (event) => { event.preventDefault(); page = 1; loadProducts(); });
filters.querySelectorAll('select').forEach((select) => select.addEventListener('change', () => { page = 1; loadProducts(); }));
$('#previous').addEventListener('click', () => { page--; loadProducts(); });
$('#next').addEventListener('click', () => { page++; loadProducts(); });
document.querySelector('[data-open="cart"]').addEventListener('click', () => run(showCart));
document.querySelector('[data-open="account"]').addEventListener('click', () => run(() => showAccount()));
panel.addEventListener('close', () => { panelRequest++; });
run(loadFilters);
loadProducts();
run(async () => updateBadge(await currentCart()));
