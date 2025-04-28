document.addEventListener("DOMContentLoaded", async function () {
    // draw charts
    updateChart();
    var currentYear = new Date().getFullYear().toString();
    console.log("current year", currentYear);
    let monthlySpendingChart = null;
    var chart = updateMonthlySpendingChart(currentYear);

    // populate year selection
    const selectElement = document.getElementById('monthly-chart-select');
    var years = await fetchTransactionDistYearsForCurrent();
    console.log(years);
    for (let i = 0; i < years.length; i++) {
        const option = document.createElement('option');
        option.value = years[i];
        option.textContent = years[i];
        selectElement.appendChild(option);
    }

    const select = document.getElementById('monthly-chart-select');

    select.addEventListener('change', function () {
        console.log("changed");
        const year = this.value;
        updateMonthlySpendingChart(year);
    });

    // download report btn
    document.getElementById('download-button')
        .addEventListener('click', async function () {
            const response = await fetch('/api/Transaction/DownloadReportForCurrent', {
                method: "GET",
                headers: {
                    "Accept": "application/pdf"
                },
                credentials: "include"
            });

            if (!response.ok) {
                console.error("Error downloading report.", response);
                displayBasicModal("Download failed!", "error");
                return;
            }

            let filename = "report.pdf";

            const disposition = response.headers.get('Content-Disposition');
            if (disposition) {
                const filenameMatch = disposition.match(/filename\*=UTF-8''(.+)/);
                if (filenameMatch && filenameMatch.length > 1) {
                    filename = decodeURIComponent(filenameMatch[1]);
                }
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);

            const a = document.createElement('a');
            a.href = url;
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            a.remove();

            window.URL.revokeObjectURL(url);
            console.log(`Downloading: ${filename}`);
        });
    async function updateMonthlySpendingChart(yearString) {
        var year = parseInt(yearString);
        const userId = document.getElementById("current-user-id").value;

        const diagramTitle = document.getElementById("finances-sum-month-area-chart-title");
        if (year) {
            diagramTitle.textContent = "Monthly Income vs Expense - " + year;
        } else {
            diagramTitle.textContent = "Monthly Income vs Expense";
        }

        const incomeTransactions = await fetchTransactions(userId, 1); // income
        const expenseTransactions = await fetchTransactions(userId, 2); // expense

        const months = [
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        ];

        const monthlySum = (transactions) => {
            const sums = Array(12).fill(0);
            transactions.forEach(tx => {
                const date = new Date(tx.date);
                if (date.getFullYear() === year) {
                    const month = date.getMonth();
                    sums[month] += parseFloat(tx.amount);
                }
            });
            return sums;
        };

        const incomeData = monthlySum(incomeTransactions);
        const expenseData = monthlySum(expenseTransactions);

        const ctx = document.getElementById("finances-sum-month-area-chart");

        if (monthlySpendingChart) {
            monthlySpendingChart.destroy();
        }

        monthlySpendingChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: months,
                datasets: [
                    {
                        label: "Income",
                        backgroundColor: "rgba(78, 223, 115, 0.7)",
                        borderColor: "rgba(78, 223, 115, 1)",
                        borderWidth: 1,
                        data: incomeData
                    },
                    {
                        label: "Expense",
                        backgroundColor: "rgba(223, 78, 78, 0.7)",
                        borderColor: "rgba(223, 78, 78, 1)",
                        borderWidth: 1,
                        data: expenseData
                    }
                ]
            },
            options: {
                maintainAspectRatio: false,
                scales: {
                    xAxes: [{
                        stacked: false,
                        scaleLabel: {
                            display: true,
                            labelString: "Month",
                            fontSize: 12,
                            fontStyle: "bold"
                        },
                        gridLines: {
                            display: false
                        }
                    }],
                    yAxes: [{
                        stacked: false,
                        ticks: {
                            callback: function (value) {
                                return 'Ft' + number_format(value);
                            }
                        },
                        scaleLabel: {
                            display: true,
                            labelString: "Amount (Ft)",
                            fontSize: 12,
                            fontStyle: "bold"
                        },
                        gridLines: {
                            color: "rgb(234, 236, 244)",
                            zeroLineColor: "rgb(234, 236, 244)",
                            drawBorder: false,
                            borderDash: [2],
                            zeroLineBorderDash: [2]
                        }
                    }]
                },
                legend: {
                    display: true
                }
            }
        });

        return monthlySpendingChart;
    }

});

async function fetchTransactions(userId, type) {
    try {
        const response = await fetch(`/api/Transaction/user/${userId}/type/${type}`);
        if (!response.ok) {
            throw new Error(`Error: ${response.status} - ${response.statusText}`);
        }
        return await response.json();
    } catch (error) {
        console.error(`Failed to fetch ${type} transactions:`, error);
        return [];
    }
}

function groupByDay(transactions) {
    let today = new Date();
    let currentYear = today.getFullYear();
    let currentMonth = today.getMonth();
    let dailyData = new Array(31).fill(0);

    transactions.forEach(transaction => {
        let date = new Date(transaction.date);
        if (date.getFullYear() === currentYear && date.getMonth() === currentMonth) {
            let dayIndex = date.getDate() - 1;
            dailyData[dayIndex] += transaction.amount;
        }
    });
    return dailyData.slice(0, today.getDate());
}

function getCurrentMonth() {
    const months = [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];
    const currentMonthIndex = new Date().getMonth();
    return months[currentMonthIndex];
}
function getDaysInMonth() {
    let today = new Date();
    let days = [];
    for (let i = 1; i <= today.getDate(); i++) {
        days.push(i + ".");
    }
    return days;
}

async function updateChart() {
    const userId = document.getElementById("current-user-id").value;

    var diagramTitle = document.getElementById("finances-curr-month-area-chart-title");
    diagramTitle.textContent = "Financial Overview - " + getCurrentMonth();

    const incomeTransactions = await fetchTransactions(userId, 1);
    const expenseTransactions = await fetchTransactions(userId, 2);

    const incomeData = groupByDay(incomeTransactions);
    const expenseData = groupByDay(expenseTransactions);
    const daysLabels = getDaysInMonth();

    var ctx = document.getElementById("finances-curr-month-area-chart");
    var chart1 = new Chart(ctx, {
        type: 'line',
        data: {
            labels: daysLabels,
            datasets: [
                {
                    label: "Income",
                    lineTension: 0.3,
                    backgroundColor: "rgba(78, 223, 115, 0.05)",
                    borderColor: "rgba(78, 223, 115, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(78, 223, 115, 1)",
                    pointBorderColor: "rgba(78, 223, 115, 1)",
                    data: incomeData
                },
                {
                    label: "Expense",
                    lineTension: 0.3,
                    backgroundColor: "rgba(223, 78, 78, 0.05)",
                    borderColor: "rgba(223, 78, 78, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(223, 78, 78, 1)",
                    pointBorderColor: "rgba(223, 78, 78, 1)",
                    data: expenseData
                }
            ]
        },
        options: {
            maintainAspectRatio: false,
            layout: {
                padding: {
                    left: 10,
                    right: 25,
                    top: 25,
                    bottom: 0
                }
            },
            scales: {
                xAxes: [{
                    time: {
                        unit: 'day'
                    },
                    gridLines: {
                        display: false,
                        drawBorder: false
                    },
                    ticks: {
                        maxTicksLimit: 10
                    },
                    scaleLabel: {
                        display: true,
                        labelString: getCurrentMonth(),
                        fontSize: 11,
                        fontStyle: "bold"
                    }
                }],
                yAxes: [{
                    ticks: {
                        maxTicksLimit: 5,
                        padding: 10,
                        callback: function (value) {
                            return 'Ft' + number_format(value);
                        }
                    },
                    gridLines: {
                        color: "rgb(234, 236, 244)",
                        zeroLineColor: "rgb(234, 236, 244)",
                        drawBorder: false,
                        borderDash: [2],
                        zeroLineBorderDash: [2]
                    },
                    scaleLabel: {
                        display: true,
                        labelString: "Amount (Ft)",
                        fontSize: 11,
                        fontStyle: "bold"
                    }
                }]
            },
            legend: {
                display: true
            },
            tooltips: {
                backgroundColor: "rgb(255,255,255)",
                bodyFontColor: "#858796",
                titleMarginBottom: 10,
                titleFontColor: '#6e707e',
                titleFontSize: 14,
                borderColor: '#dddfeb',
                borderWidth: 1,
                xPadding: 15,
                yPadding: 15,
                displayColors: true,
                intersect: false,
                mode: 'index',
                caretPadding: 10,
                callbacks: {
                    label: function (tooltipItem, chart) {
                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                        return datasetLabel + ': Ft' + number_format(tooltipItem.yLabel);
                    }
                }
            }
        }
    });
}

async function drawChartForGroup(groupId, transactions) {
    const ctx = document.getElementById(`group-chart-${groupId}`);

    if (!ctx) {
        console.error(`Canvas with id group-chart-${groupId} not found.`);
        return;
    }

    const incomeTransactions = transactions.filter(t => t.typeId === 1);
    const expenseTransactions = transactions.filter(t => t.typeId === 2);

    const incomeData = groupByDay(incomeTransactions);
    const expenseData = groupByDay(expenseTransactions);
    const daysLabels = getDaysInMonth();

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: daysLabels,
            datasets: [
                {
                    label: "Income",
                    lineTension: 0.3,
                    backgroundColor: "rgba(78, 223, 115, 0.05)",
                    borderColor: "rgba(78, 223, 115, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(78, 223, 115, 1)",
                    pointBorderColor: "rgba(78, 223, 115, 1)",
                    data: incomeData
                },
                {
                    label: "Expense",
                    lineTension: 0.3,
                    backgroundColor: "rgba(223, 78, 78, 0.05)",
                    borderColor: "rgba(223, 78, 78, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(223, 78, 78, 1)",
                    pointBorderColor: "rgba(223, 78, 78, 1)",
                    data: expenseData
                }
            ]
        },
        options: {
            maintainAspectRatio: false,
            layout: {
                padding: {
                    left: 10,
                    right: 25,
                    top: 25,
                    bottom: 0
                }
            },
            scales: {
                xAxes: [{
                    time: {
                        unit: 'day'
                    },
                    gridLines: {
                        display: false,
                        drawBorder: false
                    },
                    ticks: {
                        maxTicksLimit: 10
                    },
                    scaleLabel: {
                        display: true,
                        labelString: getCurrentMonth(),
                        fontSize: 11,
                        fontStyle: "bold"
                    }
                }],
                yAxes: [{
                    ticks: {
                        maxTicksLimit: 5,
                        padding: 10,
                        callback: function (value) {
                            return 'Ft' + number_format(value);
                        }
                    },
                    gridLines: {
                        color: "rgb(234, 236, 244)",
                        zeroLineColor: "rgb(234, 236, 244)",
                        drawBorder: false,
                        borderDash: [2],
                        zeroLineBorderDash: [2]
                    },
                    scaleLabel: {
                        display: true,
                        labelString: "Amount (Ft)",
                        fontSize: 11,
                        fontStyle: "bold"
                    }
                }]
            },
            legend: {
                display: true
            },
            tooltips: {
                backgroundColor: "rgb(255,255,255)",
                bodyFontColor: "#858796",
                titleMarginBottom: 10,
                titleFontColor: '#6e707e',
                titleFontSize: 14,
                borderColor: '#dddfeb',
                borderWidth: 1,
                xPadding: 15,
                yPadding: 15,
                displayColors: true,
                intersect: false,
                mode: 'index',
                caretPadding: 10,
                callbacks: {
                    label: function (tooltipItem, chart) {
                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                        return datasetLabel + ': Ft' + number_format(tooltipItem.yLabel);
                    }
                }
            }
        }
    });
}
