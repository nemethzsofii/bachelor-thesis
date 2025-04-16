function displayBasicModal(
    message,
    title,
    modalId = "basic-modal",
    textId = "basic-modal-text",
    titleId = "basic-modal-title",
    headerId = "basic-modal-header"
) {
    document.getElementById(textId).textContent = message;
    document.getElementById(titleId).textContent = title;
    const header = document.getElementById(headerId);
    const modalEl = document.getElementById(modalId);

    header.classList.remove("bg-success", "bg-danger", "bg-primary", "text-white");

    const lowerTitle = title.toLowerCase();
    if (lowerTitle === "success") {
        header.classList.add("bg-success", "text-white");
    } else if (lowerTitle === "error") {
        header.classList.add("bg-danger", "text-white");
    } else {
        header.classList.add("bg-primary", "text-white");
    }

    $(`#${modalId}`).modal('show');

    $(`#${modalId}`).on('hidden.bs.modal', function onModalHidden() {
        $(this).off('hidden.bs.modal', onModalHidden);
        location.reload();
    });
}


async function getCurrentUserId() {
    var currentUser = await getCurrentUser();
    var currentUserId = currentUser["id"];

    return currentUserId;
}

async function getCurrentUser() {
    try {
        const response = await fetch("/api/User/Current", {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Failed to fetch current user.");
        }

        const user = await response.json();
        return user;

    } catch (err) {
        console.error("Error fetching current user:", err);
        return null;
    }
}

async function getCategoryById(id) {
    try {
        const response = await fetch(`api/Category/${id}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Failed to fetch category.");
        }

        const category = await response.json();
        return category;

    } catch (err) {
        console.error("Error fetching category:", err);
        return null;
    }
}

async function fetchSavingsForCurrentUser() {
    try {
        var response = await fetch("/api/Saving/current", {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Could not fetch savings for current user", response);
        }

        return await response.json();
    } catch (err) {
        console.error("An error occured while fetching savings");
    }   
}

async function populateCategoryDropdown(typeId, divClass) {
    const categories = await getCategoriesByType(typeId);

    const containers = document.querySelectorAll(`.${divClass}`);

    if (!containers.length) return;

    containers.forEach(container => {
        if (!categories || categories.length === 0) {
            container.innerHTML = "<p>No categories available.</p>";
            return;
        }

        const select = document.createElement("select");
        select.name = "category";
        select.classList.add("category-dropdown");
        select.classList.add("input-category");

        const defaultOption = document.createElement("option");
        defaultOption.value = "";
        defaultOption.textContent = "Select a category";
        select.appendChild(defaultOption);

        categories.forEach(category => {
            const option = document.createElement("option");
            option.value = category.id;
            option.textContent = category.name;
            select.appendChild(option);
        });

        container.innerHTML = "";
        container.appendChild(select);
    });
}

async function getCategoriesByType(typeId) {
    try {
        var response = await fetch(`api/Category/typeId/${typeId}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Could not fetch categories", response);
        }

        return await response.json();
    } catch (err) {
        console.error("An error occured while fetching categories");
    }
}

