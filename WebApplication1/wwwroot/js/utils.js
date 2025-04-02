function displayBasicModal(message, title, modalId = "basic-modal", textId = "basic-modal-text", titleId = "basic-modal-title", headerId = "basic-modal-header"){
    document.getElementById(textId).textContent = message;
    document.getElementById(titleId).textContent = title;
    const header = document.getElementById(headerId);
    const modalEl = document.getElementById(modalId);

    header.classList.remove("bg-success", "bg-danger", "bg-primary", "text-white");

    if (title.toLowerCase() === "success") {
        header.classList.add("bg-success", "text-white");
    } else if (title.toLowerCase() === "error") {
        header.classList.add("bg-danger", "text-white");
    } else {
        header.classList.add("bg-primary", "text-white");
    }

    $(`#${modalId}`).modal('show');

    // Reload page after dismissing modal
    const onModalHidden = () => {
        modalEl.removeEventListener('hidden.bs.modal', onModalHidden);
        location.reload();
    };
    modalEl.addEventListener('hidden.bs.modal', onModalHidden);
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

