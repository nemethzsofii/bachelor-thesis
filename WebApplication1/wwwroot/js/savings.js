document.addEventListener("DOMContentLoaded", async function () {
    const currentUserId = await getCurrentUser();

    var savingsCardsContainer = document.querySelectorAll("savings-cards-container");
    if (savingsCardsContainer.classList.contains("group-savings-card-container")) {
        console.log("MyGroups.html-s js file is handling group savings");
    } else {
        var response = await fetchSavingsForCurrentUser();
        console.log(response);
    }
    
    for (let i = 0; i < response.length; i++) {
        createSavingsCard(savingsCardsContainer, response[i]);
    }

    savingsCardsContainer.addEventListener('click', async function (e) {
        console.log("clicked");
        if (e.target.classList.contains('add-savings-button')) {
            try {
                const id = e.target.dataset.id;
                await addToSavings(id);
            } catch (err) {
                console.error(err);
            }
        }

        if (e.target.classList.contains('withdraw-savings-button')) {
            try {
                const id = e.target.dataset.id;
                await withdrawFromSavings(id);
            } catch (err) {
                console.error(err);
            }
        }
    });
    async function addToSavings(id) {
        const input = document.getElementById(`amount-input-${id}`);
        const amount = parseInt(input.value);

        if (isNaN(amount) || amount <= 0) {
            displayBasicModal("Please enter a valid number! (positive integer)", "error");
            return;
        }

        try {
            const current = await fetchCurrentAmount(id);
            const newAmount = current + amount;

            await patchCurrentAmount(id, newAmount);
            location.reload();
        } catch (err) {
            console.error("Add to Savings failed:", err);
            alert("Something went wrong while adding to savings.");
        }
    }

    async function withdrawFromSavings(id) {
        const input = document.getElementById(`amount-input-${id}`);
        const amount = parseFloat(input.value);

        if (isNaN(amount) || amount <= 0) {
            alert("Please enter a valid amount to withdraw.");
            return;
        }

        try {
            const current = await fetchCurrentAmount(id);
            const newAmount = Math.max(0, current - amount);

            await patchCurrentAmount(id, newAmount);
            location.reload();
        } catch (err) {
            console.error("Withdraw from Savings failed:", err);
            alert("Something went wrong while withdrawing.");
        }
    }


    async function fetchCurrentAmount(id) {
        const res = await fetch(`/api/Saving/${id}`);
        if (!res.ok) throw new Error("Failed to fetch current amount");
        const data = await res.json();
        return data.currentAmount;
    }

    async function patchCurrentAmount(id, newAmount) {
        const res = await fetch(`/api/Saving/${id}`, {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ currentAmount: newAmount })
        });

        if (!res.ok) throw new Error("Failed to patch current amount");
    }


    function createSavingsCard(container, data) {
        try {
            const hasGoal = data.goalAmount !== null && data.goalAmount > 0;
            const progressPercent = hasGoal
                ? Math.min(100, Math.round((data.currentAmount / data.goalAmount) * 100))
                : 0;

            let progressColor = 'bg-danger';
            if (progressPercent >= 75) {
                progressColor = 'bg-success';
            } else if (progressPercent >= 25) {
                progressColor = 'bg-warning';
            }

            const progressBar = hasGoal
                ? `
            <div class="d-flex align-items-center my-2">
                <div class="progress flex-grow-1" style="height: 20px; border-radius: 10px;">
                    <div 
                        class="progress-bar ${progressColor} progress-bar-striped progress-bar-animated" 
                        role="progressbar" 
                        style="width: ${progressPercent}%; transition: width 1s ease-in-out;" 
                        aria-valuenow="${progressPercent}" 
                        aria-valuemin="0" 
                        aria-valuemax="100">
                    </div>
                </div>
                <span class="ms-2" style="min-width: 40px; text-align: right;"><strong>${progressPercent}%</strong></span>
            </div>`
                : '';

            let goalText = hasGoal
                ? `<p class="card-text"><strong>Goal:</strong> ${new Intl.NumberFormat("hu-HU", { style: "currency", currency: "HUF" }).format(data.goalAmount,)}</p>`
                : '';

            let deadlineText = data.deadline !== null
                ? `<p class="card-text"><strong>Deadline:</strong> ${data.deadline}</p>`
                : '';

            container.innerHTML += `
        <div class="card text-white mb-3 savings-card" style="
            max-width: 20rem; 
            margin: 8px; 
            border: none; 
            border-radius: 1rem; 
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            background: linear-gradient(145deg, #6fb1fc, #4364f7);
        ">
            <div class="card-header" style="
                background: linear-gradient(to right, #667eea, #764ba2);
                border-top-left-radius: 1rem;
                border-top-right-radius: 1rem;
                font-weight: bold;
                font-size: 1.1rem;
            ">
                ${data.title}
            </div>
            <div class="card-body" style="
                background-color: white; 
                color: #333; 
                border-bottom-left-radius: 1rem; 
                border-bottom-right-radius: 1rem;
                display: flex;
                flex-direction: column;
                justify-content: space-between;
            ">
                <div>
                    <h5 class="card-title" style="color: #444;">${data.description}</h5>
                    <p class="card-text"><strong>Currently have:</strong> ${new Intl.NumberFormat("hu-HU", { style: "currency", currency: "HUF" }).format(data.currentAmount,)}</p>
                    ${goalText}
                    ${deadlineText}
                    ${progressBar}
                    <div style="display: flex; align-items: center; margin-top: 0.5rem;">
                        <label for="amount-input-${data.id}" style="margin-right: 5px; white-space: nowrap;"><strong>Amount:</strong></label>
                        <input type="number" class="amount-input form-control form-control-sm" id="amount-input-${data.id}" name="amount-input-${data.id}" style="flex: 1;" />
                    </div>
                </div>

                <div class="mt-3 d-flex justify-content-center align-items-center gap-2" style="min-height: 48px;">
                    <div class="d-flex align-items-center">
                        <button class="btn btn-success btn-sm px-3 w-100 add-savings-button" data-id="${data.id}" style="margin: 2px">Add to Savings</button>
                    </div>
                    <div class="d-flex align-items-center">
                        <button class="btn btn-danger btn-sm px-3 w-100 withdraw-savings-button" data-id="${data.id}" style="margin: 2px">Withdraw</button>
                    </div>  
                </div>
            </div>
        </div>`;
        } catch (err) {
            console.error("An error occured while creating savings card", err);
        }
    }

});