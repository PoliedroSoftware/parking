window.openTicketWindow = function(htmlContent) {
    var printWindow = window.open('', '_blank', 'width=430,height=780,scrollbars=yes,resizable=yes');
    if (!printWindow) {
        alert('El navegador bloqueo la ventana de impresion. Permita popups para este sitio e intente de nuevo.');
        return;
    }

    printWindow.document.write(htmlContent);
    printWindow.document.close();
    printWindow.focus();
};
