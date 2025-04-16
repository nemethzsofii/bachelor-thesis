document.addEventListener("DOMContentLoaded", function () {
    try {
        var currentUserId = parseInt(document.getElementById("current-user-id").value, 10);
    } catch (err){
        var currentUserId = getCurrentUserId();
        console.log(currentUserId);
    }
    
    document.getElementById("basic-modal-ok").addEventListener("click", function () {
        $('#basic-modal').modal('hide');
    });

    document.body.addEventListener("click", function (event) {

        console.log("something clicked1");
        // Don't ruin MODAL behaviour
        const isModalClick = event.target.closest(".modal");
        const isDismissBtn = event.target.closest("[data-dismiss='modal']");
        if (isModalClick || isDismissBtn) return;

        

        console.log("something clicked");

        // case: ADD BUTTON
        if (event.target.classList.contains("add-btn")) {
            console.log("Add button clicked!");
            var container = event.target.closest(".input-card");
            var inputContainer = container.querySelector(".input-container");

            inputContainer.style.display = "block";
            event.target.style.display = "none";
        }

        // case: CANCEL BUTTON
        if (event.target.classList.contains("cancel-btn")) {
            console.log("cancel button clicked!");
            var container = event.target.closest(".input-card");
            var inputContainer = container.querySelector(".input-container");

            var inputAmount = container.querySelector(".input-amount");
            var inputDesc = container.querySelector(".input-desc");

            var addButton = container.querySelector(".add-btn");

            inputAmount.value = "";
            inputDesc.value = "";
            inputContainer.style.display = "none";
            addButton.style.display = "block";
        }

        // case: SAVE BUTTON
        if (event.target.classList.contains("save-btn")) {
            console.log("save button clicked!");
            var container = event.target.closest(".input-card");
            var inputAmount = container.querySelector(".input-amount").value;
            var inputDesc = container.querySelector(".input-desc").value;
            if (container.classList.contains("group-card")) {
                // case: GROUP TRANSACTION
                var groupId = parseInt(container.querySelector(".input-group").value);
            } else {
                // case: PERSONAL TRANSACTION
                var groupId = null;
            }

            if (container.classList.contains("income-card")) {
                // case: add income
                console.log("adding income!");
                createTransaction(currentUserId, parseInt(inputAmount), 1, null, inputDesc, groupId);
            } else if (container.classList.contains("expense-card")) {
                // case: add expense
                console.log("adding expense!");
                createTransaction(currentUserId, parseInt(inputAmount), 2, null, inputDesc, groupId);
            } else {
                console.log("invalid save class!");
            }
        }
    });

    async function createTransaction(userId, amount, typeId, categoryId, description, groupId) {
        const transactionData = {
            userId: userId,
            amount: amount,
            typeId: typeId,
            categoryId: categoryId,
            description: description,
            groupId: groupId
        };

        console.log(transactionData);

        try {
            const response = await fetch("/api/Transaction", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(transactionData)
            });

            if (!response.ok) {
                throw new Error(`Error: ${response.status} - ${response.statusText}`);
            } else {
                displayBasicModal("Transaction created successfully!", "success");
            }

            var result = await response.json();
            console.log("Transaction created successfully:", result);
        } catch (error) {
            console.error("Failed to create transaction:", error);
            displayBasicModal("Failed to create transaction!", "error");
        }
    }

    
});
