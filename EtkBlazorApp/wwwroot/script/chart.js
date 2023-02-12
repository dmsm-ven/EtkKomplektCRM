function GenerateChart(seriesNames, serisData) {
    var canvas = document.getElementById('myChart');
    var parent = canvas.parentElement;

    canvas.remove();
    parent.innerHTML = '<canvas id="myChart"></canvas>';
    canvas = document.getElementById('myChart');

    var ctx = canvas.getContext('2d');

    const data = {
        labels: seriesNames,
        datasets: serisData
    };

    const config = {
        type: 'line',
        data: data,
    };

    var myChart = new Chart(ctx, config);
}