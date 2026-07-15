window.openTicketWindow = function(htmlContent) {
    var printWindow = window.open('', '_blank', 'width=300,height=600');
    printWindow.document.write(htmlContent);
    printWindow.document.close();
    printWindow.focus();
};
