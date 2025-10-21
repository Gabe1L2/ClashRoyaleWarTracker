// Global variables for player modal
let selectedPlayerId = null;
let selectedPlayerName = '';
let selectedPlayerTag = '';
let warHistoryWasEdited = false;

// Configuration - will be set by each page
let playerModalConfig = {
    canModifyPlayerData: false,
    handlerPrefix: '', // e.g., '' for Index, '' for Rosters
    onModalClose: null // Optional callback when modal closes
};

/**
 * Initialize the player actions modal with page-specific configuration
 */
function initializePlayerActionsModal(config) {
    playerModalConfig = { ...playerModalConfig, ...config };

    // Set up modal close event listener
    const playerActionsModal = document.getElementById('playerActionsModal');
    if (playerActionsModal) {
        playerActionsModal.addEventListener('hidden.bs.modal', function () {
            // Only reload if war history was edited
            if (warHistoryWasEdited) {
                location.reload();
            }

            // Call custom callback if provided
            if (playerModalConfig.onModalClose) {
                playerModalConfig.onModalClose(warHistoryWasEdited);
            }
        });
    }

    // Set up character count listener for notes
    document.addEventListener('input', function (e) {
        if (e.target && e.target.id === 'playerNotesTextarea') {
            updateNotesCharCount();
        }
    });
}

/**
 * Open the player actions modal for a specific player
 */
function selectPlayer(playerId, playerName, playerTag, playerStatus = 'Active', playerNotes = '') {
    selectedPlayerId = playerId;
    selectedPlayerName = playerName;
    selectedPlayerTag = playerTag;
    warHistoryWasEdited = false;

    // Update modal header
    document.getElementById('selectedPlayerName').textContent = playerName;
    document.getElementById('playerTagText').textContent = `#${playerTag}`;
    document.getElementById('playerTagLink').href = `https://royaleapi.com/player/${playerTag}`;

    // Set the status dropdown
    const statusSelect = document.getElementById('playerStatusSelect');
    if (statusSelect) {
        statusSelect.value = playerStatus || 'Active';
    }

    // Populate notes
    const notesDisplay = document.getElementById('playerNotesDisplay');
    const notesEditContainer = document.getElementById('playerNotesEditContainer');
    const notesViewControls = document.getElementById('playerNotesViewControls');

    if (notesDisplay) notesDisplay.textContent = playerNotes || 'No notes available.';
    if (notesEditContainer) notesEditContainer.classList.add('d-none');
    if (notesViewControls) notesViewControls.classList.remove('d-none');

    // Reset tabs: activate status tab
    resetModalTabs();

    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('playerActionsModal'));
    modal.show();
}

/**
 * Reset all tabs to default state (Status tab active)
 */
function resetModalTabs() {
    // Clear active on all tabs/panes
    const tabButtons = document.querySelectorAll('#playerActionTabs .nav-link');
    tabButtons.forEach(btn => {
        btn.classList.remove('active');
        btn.setAttribute('aria-selected', 'false');
    });

    const tabPanes = document.querySelectorAll('#playerActionTabContent .tab-pane');
    tabPanes.forEach(p => {
        p.classList.remove('show', 'active');
    });

    // Activate status tab/pane
    const statusTab = document.getElementById('status-tab');
    const statusPane = document.getElementById('status-pane');
    if (statusTab) {
        statusTab.classList.add('active');
        statusTab.setAttribute('aria-selected', 'true');
    }
    if (statusPane) {
        statusPane.classList.add('show', 'active');
    }
}

/**
 * Update player status
 */
async function updatePlayerStatus() {
    if (!playerModalConfig.canModifyPlayerData) {
        alert('You do not have permission to modify player data.');
        return;
    }

    const status = document.getElementById('playerStatusSelect').value;

    try {
        const response = await fetch(`${playerModalConfig.handlerPrefix}?handler=UpdatePlayerStatus`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: `playerId=${selectedPlayerId}&status=${encodeURIComponent(status)}`
        });

        if (response.ok) {
            location.reload();
        } else {
            alert('Error updating player status');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Error updating player status');
    }
}

/**
 * Load player war histories
 */
async function loadPlayerWarHistories() {
    const loadingDiv = document.getElementById('warHistoryLoading');
    const contentDiv = document.getElementById('warHistoryContent');

    loadingDiv.classList.remove('d-none');
    contentDiv.innerHTML = '';

    try {
        const response = await fetch(`${playerModalConfig.handlerPrefix}?handler=PlayerWarHistories&playerId=${selectedPlayerId}`);
        const result = await response.json();

        if (result.success) {
            displayWarHistories(result.data);
        } else {
            contentDiv.innerHTML = `<div class="alert alert-danger">${result.message}</div>`;
        }
    } catch (error) {
        console.error('Error:', error);
        contentDiv.innerHTML = '<div class="alert alert-danger">Error loading war histories</div>';
    } finally {
        loadingDiv.classList.add('d-none');
    }
}

/**
 * Display war histories in a table
 */
function displayWarHistories(warHistories) {
    const contentDiv = document.getElementById('warHistoryContent');

    if (!warHistories || warHistories.length === 0) {
        contentDiv.innerHTML = '<div class="alert alert-info">No war history found for this player.</div>';
        return;
    }

    let html = `
        <div class="table-responsive" style="max-height: 400px; overflow-y: auto;">
            <table class="table table-sm table-striped">
                <thead class="table-dark sticky-top">
                    <tr>
                        <th>Season</th>
                        <th>Week</th>
                        <th>Clan</th>
                        <th>Fame</th>
                        <th>Attacks</th>
                        <th>Boat Attacks</th>
                        <th>Fame/Attack</th>
                        ${playerModalConfig.canModifyPlayerData ? '<th>Actions</th>' : ''}
                    </tr>
                </thead>
                <tbody>
    `;

    warHistories.forEach(history => {
        const ratio = history.decksUsed > 0 ? (history.fame / history.decksUsed).toFixed(1) : '0.0';
        const ratioColor = ratio >= 200 ? 'success' : ratio >= 175 ? 'warning' : 'danger';

        html += `
            <tr data-war-history-id="${history.id}">
                <td>${history.seasonID}</td>
                <td>${history.weekIndex}</td>
                <td>${history.clanName}</td>
                <td><span class="fw-bold">${history.fame}</span></td>
                <td><span class="fw-bold">${history.decksUsed}</span></td>
                <td><span class="fw-bold">${history.boatAttacks}</span></td>
                <td><span class="badge bg-${ratioColor}">${ratio}</span></td>
                ${playerModalConfig.canModifyPlayerData ?
                `<td><button class="btn btn-sm btn-outline-primary" onclick="editWarHistory(${history.id}, ${history.fame}, ${history.decksUsed}, ${history.boatAttacks}, ${history.seasonID}, ${history.weekIndex})"><i class="fas fa-edit"></i></button></td>`
                : ''}
            </tr>
        `;
    });

    html += `
                </tbody>
            </table>
        </div>
    `;

    contentDiv.innerHTML = html;
}

/**
 * Edit war history entry
 */
function editWarHistory(warHistoryId, currentFame, currentDecks, currentBoats, seasonID, weekIndex) {
    if (!playerModalConfig.canModifyPlayerData) {
        alert('You do not have permission to modify war history.');
        return;
    }

    // Prefill modal fields
    document.getElementById('editWarHistoryId').value = warHistoryId;
    document.getElementById('editFame').value = Number(currentFame) ?? 0;
    document.getElementById('editDecksUsed').value = Number(currentDecks) ?? 0;
    document.getElementById('editBoatAttacks').value = Number(currentBoats) ?? 0;
    document.getElementById('editWarHistoryError').classList.add('d-none');
    document.getElementById('editWarHistoryError').textContent = '';

    const modalTitle = document.querySelector('#editWarHistoryModal .modal-title');
    modalTitle.textContent = `Edit War History - ${seasonID}-${weekIndex}`;

    const playerActionsEl = document.getElementById('playerActionsModal');
    const editEl = document.getElementById('editWarHistoryModal');

    // Fade the Player Actions modal
    if (playerActionsEl) {
        playerActionsEl.classList.add('faded-behind');
    }

    // Create edit modal instance
    const editModal = new bootstrap.Modal(editEl, { backdrop: false, keyboard: true });
    editModal.show();

    // When edit modal hides, remove faded class
    const onHidden = () => {
        if (playerActionsEl) {
            playerActionsEl.classList.remove('faded-behind');
        }
        editEl.removeEventListener('hidden.bs.modal', onHidden);
    };
    editEl.addEventListener('hidden.bs.modal', onHidden);
}

/**
 * Save war history from modal
 */
async function saveWarHistoryFromModal() {
    const saveBtn = document.getElementById('saveWarHistoryBtn');
    const errorEl = document.getElementById('editWarHistoryError');

    const warHistoryId = parseInt(document.getElementById('editWarHistoryId').value, 10);
    const fame = parseInt(document.getElementById('editFame').value, 10);
    const decksUsed = parseInt(document.getElementById('editDecksUsed').value, 10);
    const boatAttacks = parseInt(document.getElementById('editBoatAttacks').value, 10);

    // Validation
    if (Number.isNaN(fame) || fame < 0 || fame > 3600) {
        showEditError('Fame must be between 0 and 3600.');
        return;
    }
    if (Number.isNaN(decksUsed) || decksUsed < 0 || decksUsed > 16) {
        showEditError('Attacks (Decks Used) must be between 0 and 16.');
        return;
    }
    if (Number.isNaN(boatAttacks) || boatAttacks < 0 || boatAttacks > 16) {
        showEditError('Boat Attacks must be a non-negative integer.');
        return;
    }

    saveBtn.disabled = true;
    const originalHtml = saveBtn.innerHTML;
    saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Saving...';

    try {
        const body = `warHistoryId=${encodeURIComponent(warHistoryId)}&fame=${encodeURIComponent(fame)}&decksUsed=${encodeURIComponent(decksUsed)}&boatAttacks=${encodeURIComponent(boatAttacks)}`;

        const response = await fetch(`${playerModalConfig.handlerPrefix}?handler=UpdateWarHistory`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body
        });

        const result = await response.json();

        if (response.ok && result.success) {
            warHistoryWasEdited = true;

            // Close edit modal
            const modalEl = document.getElementById('editWarHistoryModal');
            const bsModal = bootstrap.Modal.getInstance(modalEl);
            if (bsModal) bsModal.hide();

            // Refresh war history list
            loadPlayerWarHistories();
        } else {
            showEditError(result?.message ?? 'Failed to update war history.');
        }
    } catch (err) {
        console.error(err);
        showEditError('An error occurred while updating the war history.');
    } finally {
        saveBtn.disabled = false;
        saveBtn.innerHTML = originalHtml;
    }
}

function showEditError(message) {
    const errorEl = document.getElementById('editWarHistoryError');
    errorEl.textContent = message;
    errorEl.classList.remove('d-none');
}

/**
 * Notes management functions
 */
function startEditPlayerNotes() {
    const display = document.getElementById('playerNotesDisplay');
    const editContainer = document.getElementById('playerNotesEditContainer');
    const viewControls = document.getElementById('playerNotesViewControls');
    const textarea = document.getElementById('playerNotesTextarea');

    const notes = display.textContent === 'No notes available.' ? '' : display.textContent;
    textarea.value = notes;
    updateNotesCharCount();

    display.classList.add('d-none');
    viewControls.classList.add('d-none');
    editContainer.classList.remove('d-none');
    textarea.focus();
}

function cancelEditPlayerNotes() {
    const display = document.getElementById('playerNotesDisplay');
    const editContainer = document.getElementById('playerNotesEditContainer');
    const viewControls = document.getElementById('playerNotesViewControls');
    const error = document.getElementById('playerNotesError');

    error.classList.add('d-none');
    editContainer.classList.add('d-none');
    display.classList.remove('d-none');
    viewControls.classList.remove('d-none');
}

function updateNotesCharCount() {
    const textarea = document.getElementById('playerNotesTextarea');
    const counter = document.getElementById('notesCharCount');
    const len = textarea ? textarea.value.length : 0;
    if (counter) counter.textContent = `${len} / 100`;
}

async function savePlayerNotes() {
    if (!playerModalConfig.canModifyPlayerData) {
        alert('You do not have permission to modify player notes.');
        return;
    }

    const textarea = document.getElementById('playerNotesTextarea');
    const error = document.getElementById('playerNotesError');
    const saveBtn = document.getElementById('savePlayerNotesBtn');

    const notes = textarea.value.trim();
    if (notes.length > 100) {
        error.textContent = 'Notes must be 100 characters or fewer.';
        error.classList.remove('d-none');
        return;
    }

    saveBtn.disabled = true;
    const original = saveBtn.innerHTML;
    saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i> Saving...';

    try {
        const body = `playerId=${encodeURIComponent(selectedPlayerId)}&notes=${encodeURIComponent(notes)}`;
        const response = await fetch(`${playerModalConfig.handlerPrefix}?handler=UpdatePlayerNotes`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body
        });

        const result = await response.json();
        if (response.ok && result.success) {
            // Update modal display
            const display = document.getElementById('playerNotesDisplay');
            display.textContent = notes || 'No notes available.';

            cancelEditPlayerNotes();

            // Optional: notify page to update notes display
            if (window.updatePlayerNotesInTable) {
                window.updatePlayerNotesInTable(selectedPlayerId, notes);
            }
        } else {
            error.textContent = result?.message ?? 'Failed to save notes.';
            error.classList.remove('d-none');
        }
    } catch (err) {
        console.error(err);
        error.textContent = 'An error occurred while saving notes.';
        error.classList.remove('d-none');
    } finally {
        saveBtn.disabled = false;
        saveBtn.innerHTML = original;
    }
}

/**
 * Helper function to get anti-forgery token
 */
function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}