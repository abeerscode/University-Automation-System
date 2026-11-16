document.addEventListener('submit', e => {
  const form = e.target;
  if (!form || form.dataset.submitting === 'yes') { e.preventDefault(); return; }
  form.dataset.submitting = 'yes';
  const button = form.querySelector('button[type=submit],button:not([type])');
  if (button) { button.disabled = true; }
});
