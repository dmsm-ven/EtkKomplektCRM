function GenerateChart(title, labelNames, pointsArray) {
    var canvas = document.getElementById('myChart');
    var parent = canvas.parentElement;
   
    canvas.remove();
    parent.innerHTML = '<canvas id="myChart"></canvas>';
    canvas = document.getElementById('myChart');

    var ctx = canvas.getContext('2d');

    var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labelNames,
            datasets: [{
                label: title,
                data: pointsArray,
                backgroundColor: 'rgba(255, 99, 132, 0.2)',
                borderColor: 'rgba(255, 99, 132, 1)',
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}