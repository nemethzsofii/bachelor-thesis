document.addEventListener("DOMContentLoaded", async function () {
    const currentUserId = await getCurrentUser();
    const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));
    await sleep(1000); //wait till group cards get generated
    var savingsCardsContainers = document.querySelectorAll(".savings-cards-container");

    for (let i = 0; i < savingsCardsContainers.length; i++) {
        console.log(savingsCardsContainers[i]);
        if (savingsCardsContainers[i].classList.contains("group-savings-card-container")) {
            console.log("group savings");
            var response = await fetchSavingsForGroup(parseInt(savingsCardsContainers[i].id));
        } else {
            console.log("personal");
            var response = await fetchSavingsForCurrentUser();
        }

        for (let j = 0; j < response.length; j++) {
            createSavingsCard(savingsCardsContainers[i], response[j]);
        }

        savingsCardsContainers[i].addEventListener('click', async function (e) {
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
    }

    $('#savingsDeleteModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget)
        var savingId = button.data('saving-id')
       
        console.log(savingId)

        var modal = $(this)
        modal.find('.modal-hidden-saving-id').val(savingId)
    })

    var savingsDeleteButton = document.getElementById("savings-delete-button");
    savingsDeleteButton.addEventListener("click", async function () {
        var savingId = parseInt(document.querySelector(".modal-hidden-saving-id").value);
        deleteSaving(savingId);
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
        console.log(data);
        try {
            const hasGoal = data.goalAmount !== null && data.goalAmount > 0;
            const progressPercent = hasGoal
                ? Math.min(100, Math.round((data.currentAmount / data.goalAmount) * 100))
                : 0;

            const cardBackground = progressPercent >= 100
                ? "lightgreen"
                : "white";

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
                ? `<p class="card-text"><strong>Goal:</strong> ${new Intl.NumberFormat('hu-HU', { minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(data.goalAmount)} Ft</p>`
                : '';

            let deadlineText = data.deadline !== null
                ? `<p class="card-text"><strong>Deadline:</strong> ${data.deadline.split('T')[0]}</p>`
                : '';

            container.innerHTML += `
        <div class="card text-white mb-3 savings-card" style="
            width: 19rem; 
            margin: 8px; 
            border: none; 
            border-radius: 1rem; 
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        ">
            <div class="card-header py-3 d-flex justify-content-between align-items-center" style="
                background: linear-gradient(to right, #667eea, #764ba2);
                border-top-left-radius: 1rem;
                border-top-right-radius: 1rem;
                font-weight: bold;
                font-size: 1.1rem;
            ">
                ${data.title}
                <button type="button" id="add-button" class="btn text-white" data-toggle="modal" data-target="#savingsDeleteModal" data-saving-id="${data.id}" style="background: transparent; border: none;">
                    <i class="bi bi-trash"></i>
                </button>

            </div>
            <div class="card-body" style="
                background-color: ${cardBackground} !important; 
                color: #333; 
                border-bottom-left-radius: 1rem; 
                border-bottom-right-radius: 1rem;
                display: flex;
                flex-direction: column;
                justify-content: space-between;
            ">
                <div>
                    <h5 class="card-title" style="color: #444;">${data.description}</h5>
                    <p class="card-text"><strong>Currently have:</strong> ${new Intl.NumberFormat('hu-HU', { minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(data.currentAmount)} Ft</p>
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