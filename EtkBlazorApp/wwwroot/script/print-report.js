function PrintHtml(htmlContent) {
        var printWindow = window.open('', '', 'width=960, height=680');
        printWindow.document.write('<html><body><br />');
        printWindow.document.write(htmlContent);
        printWindow.document.write('</body></html>');
        printWindow.close();
        printWindow.print();
    }