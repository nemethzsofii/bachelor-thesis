document.addEventListener("DOMContentLoaded", async function () {
    function number_format(number, decimals = 0) {
        if (isNaN(number)) return '0 Ft';

        const fixed = Number(number).toFixed(decimals);
        let [intPart, decPart] = fixed.split('.');

        intPart = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');

        const formatted = decPart ? `${intPart},${decPart}` : intPart;
        return `${formatted} Ft`;
    }

    var currentUserId = await getCurrentUserId();

    var monthlySpendingLimit = await getMonthlySpendingLimit(currentUserId);
    const formatted = monthlySpendingLimit.toLocaleString('hu-HU');

    document.getElementById("spending-limit-text").textContent = formatted;

    console.log(monthlySpendingLimit);

    loadMonthlySpendingChart();

    async function loadMonthlySpendingChart() {
        console.log(currentUserId);
        const limit = await getMonthlySpendingLimit(currentUserId);

        const allExpenses = await listAllTransactions(currentUserId, 2); // 2 - expense

        const now = new Date();
        const currentMonth = now.getMonth();
        const currentYear = now.getFullYear();

        const currentMonthExpenses = allExpenses.filter(tx => {
            const txDate = new Date(tx.date);
            return txDate.getMonth() === currentMonth && txDate.getFullYear() === currentYear;
        });

        const totalSpent = currentMonthExpenses.reduce((sum, tx) => {
            return sum + parseFloat(tx.amount || 0);
        }, 0);

        const remaining = Math.max(limit - totalSpent, 0);

        const ctx = document.getElementById("monthly-spending-pie-chart");
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: ['Spent', 'Remaining'],
                datasets: [{
                    data: [totalSpent, remaining],
                    backgroundColor: [
                        'rgba(255, 206, 86, 0.7)',
                        'rgba(54, 162, 235, 0.7)'
                    ],
                    borderColor: [
                        'rgba(255, 206, 86, 1)',
                        'rgba(54, 162, 235, 1)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                plugins: {
                    tooltip: {
                        enabled: true,
                        callbacks: {
                            label: function (context) {
                                const label = context.label ?? '';
                                const value = context.raw ?? 0;
                                return `${label}: ${number_format(value)}`;
                            }
                        }
                    },
                    legend: {
                        display: true
                    }
                }
            }
        });
    }
});


