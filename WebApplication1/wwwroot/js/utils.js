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
    } else if (lowerTitle == "warning") {
        header.classList.add("bg-warning", "text-white");
    }else {
        header.classList.add("bg-primary", "text-white");
    }

    $(`#${modalId}`).modal('show');

    $(`#${modalId}`).on('hidden.bs.modal', function onModalHidden() {
        $(this).off('hidden.bs.modal', onModalHidden);
        location.reload();
    });
}
async function changeSpendingLimit(userId, newValue) {
    try {
        const response = await fetch(`/api/User/${userId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: {
                "MonthlySpendingLimit": newValue
            }
        });

        if (!response.ok) {
            throw new Error("Failed to update monthly spending limit.");
        }

    } catch (err) {
        console.error("Error updating monthly spending limit:", err);
        return null;
    }
}
async function updateUser(id, updateData) {
    const response = await fetch(`/api/User/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        },
        credentials: "include",
        body: JSON.stringify(updateData)
    });

    if (!response.ok) {
        const errorMessage = await response.json();
        if (errorMessage["message"] == "Invalid email format.") {
            alert("Invalid email format!");
        } else if (errorMessage["message"] == "Username already taken.") {
            alert("Username already taken!");
        } else {
            console.log(errorMessage);
            alert("Something went wrong!");
        }
        console.error(`Error: ${response.status} - ${errorMessage}`);
        return "User update failed.";
    } else {
        location.reload();
        return "User updated successfully!";
    }
}
async function getMonthlySpendingLimit(userId) {
    try {
        const response = await fetch(`/api/User/${userId}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Failed to fetch user.");
        }

        const user = await response.json();
        try {
            limit = user["monthlySpendingLimit"];
            if (limit) {
                return parseInt(limit);
            }
            return 0;
        } catch (err) {
            console.log(err);
            return -1;
        }

    } catch (err) {
        console.error("Error fetching current user:", err);
        return null;
    }
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
        const response = await fetch(`/api/Category/${id}`, {
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

async function fetchSavingsForGroup(groupId) {
    try {
        var response = await fetch(`/api/Saving/groupid/${groupId}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Could not fetch savings for group", response);
        }

        return await response.json();
    } catch (err) {
        console.error("An error occured while fetching savings for group");
    }
}

async function fetchGroupMembers(groupId) {
    try {
        var response = await fetch(`/api/GroupMembership/ByGroup/${groupId}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Could not fetch memberships for group", response);
        }

        return await response.json();
    } catch (err) {
        console.error("An error occured while fetching memberships for group");
    }
}

async function deleteSaving(id) {
    const res = await fetch(`/api/Saving/${id}`, {
        method: "DELETE",
        headers: {
            "Content-Type": "application/json"
        }
    });

    if (!res.ok) throw new Error("Failed to delete saving");
    location.reload();
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
        var response = await fetch(`/api/Category/typeId/${typeId}`, {
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

async function getUserById(userId) {
    try {
        var response = await fetch(`/api/User/${userId}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Could not fetch user", response);
        }

        return await response.json();
    } catch (err) {
        console.error("An error occured while fetching user");
        return null;
    }
}

async function getGroupById(groupId) {
    try {
        var response = await fetch(`/api/Group/${groupId}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Could not fetch group", response);
        }

        return await response.json();
    } catch (err) {
        console.error("An error occured while fetching group");
        return null;
    }
}

async function fetchTransactionDistYearsForCurrent() {
    try {
        const currentUserId = await getCurrentUserId();
        var response = await fetch(`/api/Transaction/DistinctYears/${currentUserId}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Could not fetch distinct transaction years", response);
        }

        return await response.json();
    } catch (err) {
        console.error("An error occured while fetching distinct transaction years");
    }
}

async function listAllTransactions(userId, typeId, group) {
    try {
        var response = "";
        if (!typeId && !group) {
            response = await fetch(`api/Transaction/user/${userId}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json"
                }
            });
        } else if (!group) {
            response = await fetch(`api/Transaction/user/${userId}/type/${typeId}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json"
                }
            });
        } else if (!typeId) {
            response = await fetch(`api/Transaction/user/${userId}/group/${group}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json"
                }
            });
        }

        if (!response.ok) {
            throw new Error(`Error: ${response.status} - ${response.statusText}`);
        }

        var result = await response.json();
        console.log("Listed transactions successfully:", result);
        return result;
    } catch (error) {
        console.error("Failed to list transactions:", error);
        return null;
    }
}

async function postInvite(groupId, senderUserId, receiverUserId) {
    try {
        const response = await fetch("/api/Invite", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                SenderUserId: senderUserId,
                ReceiverUserId: receiverUserId,
                GroupId: groupId
            })
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error(`Error ${response.status}: ${errorText}`);
        }
        return response.status;
    } catch (error) {
        console.log(error);
    }
}

async function deleteGroup(groupId) {
    try {
        const response = await fetch(`/api/Group/${groupId}`, {
            method: "DELETE",
            headers: { "Content-Type": "application/json" },
            credentials: 'include'
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error(`Error ${response.status}: ${errorText}`);
        }
        return response.status;
    } catch (error) {
        console.log(error);
    }
}

async function getUserByUsername(username) {
    try {
        const response = await fetch(`/api/User/username/${username}`, {
            method: "GET",
            headers: { "Content-Type": "application/json" }
        });
        if (!response.ok) return null;
        return response.json();

    } catch (error) {
        console.log(error);
    }
}

async function getGroupsForCurrentUser() {
    const response = await fetch("/api/GroupMembership/ByUser", {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        },
        credentials: "include"
    });

    if (!response.ok) {
        const errorMessage = await response.json();
        displayBasicModal("Something went wrong!", "Error");
        console.error(`Error: ${response.status} - ${errorMessage}`);
    } else {
        const result = await response.json();
        return result;
    }
}

async function getUserById(id) {
    const response = await fetch(`/api/User/${id}`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        },
        credentials: "include"
    });

    if (!response.ok) {
        const errorMessage = await response.json();
        console.error(`Error: ${response.status} - ${errorMessage}`);
        return null;
    } else {
        const result = await response.json()
        return result;
    }
}
async function getRoleById(id) {
    const response = await fetch(`/api/Role/${id}`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        },
        credentials: "include"
    });

    if (!response.ok) {
        const errorMessage = await response.json();
        displayBasicModal("Something went wrong!", "Error");
        console.error(`Error: ${response.status} - ${errorMessage}`);
        return null;
    } else {
        const result = await response.json()
        return result;
    }
}
async function getGroupSavings(id) {
    const response = await fetch(`/api/Saving/groupid/${id}`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        },
        credentials: "include"
    });

    if (!response.ok) {
        const errorMessage = await response.json();
        displayBasicModal("Something went wrong!", "Error");
        console.error(`Error: ${response.status} - ${errorMessage}`);
        return null;
    } else {
        const result = await response.json()
        return result;
    }
}

async function getGroupById(id) {
    const response = await fetch(`/api/Group/${id}`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        },
        credentials: "include"
    });

    if (!response.ok) {
        const errorMessage = await response.json();
        displayBasicModal("Something went wrong!", "Error");
        console.error(`Error: ${response.status} - ${errorMessage}`);
        return null;
    } else {
        const result = await response.json()
        return result;
    }
}

async function listAllTransactions(userId, type, groupId) {
    try {
        var response = "";
        if (!type && !groupId && userId) {
            response = await fetch(`api/Transaction/user/${userId}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json"
                }
            });
        } else if (!type && !userId && groupId) {
            response = await fetch(`/api/Transaction/group/${groupId}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json"
                }
            });
        } else if (!groupId && type && userId) {
            response = await fetch(`api/Transaction/user/${userId}/type/${type}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json"
                }
            });
        } else if (!type && userId && groupId) {
            response = await fetch(`api/Transaction/user/${userId}/group/${group}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json"
                }
            });
        }

        if (!response.ok) {
            throw new Error(`Error: ${response.status} - ${response.statusText}`);
        }

        var result = await response.json();
        console.log("Listed transactions successfully:", result);
        return result;
    } catch (error) {
        console.error("Failed to list transactions:", error);
        return null;
    }
}
