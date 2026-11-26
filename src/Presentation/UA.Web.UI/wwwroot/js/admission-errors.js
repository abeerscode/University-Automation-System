document.getElementById('download-import-errors')?.addEventListener('click', () => {
 const lines = [...document.querySelectorAll('#import-errors li')].map(x => '"' + x.textContent.replaceAll('"','""') + '"');
 const url = URL.createObjectURL(new Blob(['\ufeffError\r\n'+lines.join('\r\n')],{type:'text/csv;charset=utf-8'}));
 const a = document.createElement('a'); a.href=url; a.download='admission-import-errors.csv'; a.click(); setTimeout(()=>URL.revokeObjectURL(url),1000);
});