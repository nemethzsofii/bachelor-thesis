function displayModal(message, title, modalId = "basic-modal", textId = "basic-modal-text", titleId = "basic-modal-title", headerId = "basic-modal-header"){
    document.getElementById(textId).textContent = message;
    document.getElementById(titleId).textContent = title;
    const header = document.getElementById(headerId);

    // Remove any previous styling
    header.classList.remove("bg-success", "bg-danger", "bg-primary", "text-white");

    // Apply styles based on title
    if (title.toLowerCase() === "success") {
        header.classList.add("bg-success", "text-white");
    } else if (title.toLowerCase() === "error") {
        header.classList.add("bg-danger", "text-white");
    } else {
        header.classList.add("bg-primary", "text-white");
    }

    $(`#${modalId}`).modal('show');
}
