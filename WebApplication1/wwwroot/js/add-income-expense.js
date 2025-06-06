﻿document.addEventListener("DOMContentLoaded", async function () {
    var remaining = null;
    try {
        var currentUserId = parseInt(document.getElementById("current-user-id").value, 10);
    } catch (err){
        var currentUserId = getCurrentUserId();
        console.log(currentUserId);
    }
    
    document.getElementById("basic-modal-ok").addEventListener("click", function () {
        $('#basic-modal').modal('hide');
    });

    document.body.addEventListener("click", async function (event) {

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
            if (inputDesc) {
                inputDesc.value = "";
            }
            
            inputContainer.style.display = "none";
            addButton.style.display = "block";
        }

        // case: SAVE BUTTON
        if (event.target.classList.contains("save-btn")) {
            console.log("save button clicked!");
            var container = event.target.closest(".input-card");
            var inputAmount = container.querySelector(".input-amount").value;
            var inputDescElement = container.querySelector(".input-desc");
            var categoryIdElement = container.querySelector(".input-category");
            if (inputDescElement && categoryIdElement) {
                var inputDesc = inputDescElement.value;
                var categoryId = categoryIdElement.value;
                if (categoryId == "") {
                    categoryId = null;
                }
            }
            
            if (container.classList.contains("group-card")) {
                // case: GROUP TRANSACTION
                var groupId = parseInt(container.querySelector(".input-group").value);
            } else {
                // case: PERSONAL TRANSACTION
                var groupId = null;
                remaining = await getRemainingMonthlySpending();
            }

            if (container.classList.contains("income-card")) {
                // case: add income
                console.log("adding income!");
                createTransaction(currentUserId, parseInt(inputAmount), 1, categoryId, inputDesc, groupId);
            } else if (container.classList.contains("expense-card")) {
                // case: add expense
                console.log("adding expense!");
                createTransaction(currentUserId, parseInt(inputAmount), 2, categoryId, inputDesc, groupId);
            } else if (container.classList.contains("spending-card")) {
                // case: change spending limit
                console.log("changing spending limit!");
                try {
                    updateUser(currentUserId, { "MonthlySpendingLimit": parseInt(inputAmount) });
                } catch (err) {
                    console.log(err);
                }
                
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
            groupId: groupId,
            date: new Date().toISOString()
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
                console.log(typeId);
                if (remaining != null && typeId == 2) {
                    remaining = await getRemainingMonthlySpending();
                    
                    if (remaining < 0) {
                        const formatted = (-remaining).toLocaleString('hu-HU');
                        displayBasicModal(`Monthly limit overstepped by ${formatted} Ft! `, "warning");
                    } else {
                        const formatted = remaining.toLocaleString('hu-HU');
                        displayBasicModal(`Transaction created! Remaining monthly limit: ${formatted} Ft! `, "success");
                    }
                } else {
                    displayBasicModal("Transaction created successfully! ", "success");
                }
            }

            var result = await response.json();

            console.log("Transaction created successfully:", result);
        } catch (error) {
            console.error("Failed to create transaction:", error);
            displayBasicModal("Failed to create transaction!", "error");
        }
    }

    async function getRemainingMonthlySpending(){
        const limit = await getMonthlySpendingLimit(currentUserId);
        if (limit == 0) {
            return null;
        }

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

        const remaining = limit - totalSpent;

        return remaining; 
    }

    
});
