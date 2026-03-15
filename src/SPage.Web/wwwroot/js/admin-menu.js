document.addEventListener('DOMContentLoaded', () => {
    const treeRoot = document.getElementById('menu-tree');
    const saveBtn = document.getElementById('save-order-btn');

    if (!treeRoot || !saveBtn) return;

    makeSortable(treeRoot, saveBtn);

    saveBtn.addEventListener('click', async () => {
        const payload = {
            items: buildMenuPayload()
        };

        const reorderUrl = treeRoot.getAttribute('data-reorder-url');
        const tokenInput = document.getElementById('menu-reorder-token');
        const token = tokenInput ? tokenInput.value : '';

        if (!reorderUrl) {
            console.error('Reorder URL is missing.');
            return;
        }

        try {
            const response = await fetch(reorderUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                alert('Lưu sắp xếp thất bại.');
                return;
            }

            saveBtn.disabled = true;
        } catch (e) {
            console.error(e);
            alert('Không thể lưu sắp xếp.');
        }
    });
});

function buildMenuPayload() {
    const result = [];

    function walkList(ul, parentId) {
        const items = ul.querySelectorAll(':scope > li[data-id]');
        items.forEach((li, index) => {
            const id = parseInt(li.getAttribute('data-id'), 10);
            result.push({
                id: id,
                parentId: parentId,
                sortOrder: index
            });

            const childUl = li.querySelector(':scope > ul');
            if (childUl) {
                walkList(childUl, id);
            }
        });
    }

    const root = document.querySelector('#menu-tree > ul');
    if (root) {
        walkList(root, null);
    }

    return result;
}

function makeSortable(container, saveBtn) {
    let dragged;

    container.addEventListener('dragstart', (e) => {
        const li = e.target.closest('li[data-id]');
        if (!li) return;
        dragged = li;
        e.dataTransfer.effectAllowed = 'move';
        li.classList.add('opacity-50');
    });

    container.addEventListener('dragend', () => {
        if (dragged) {
            dragged.classList.remove('opacity-50');
            dragged = null;
        }
    });

    container.addEventListener('dragover', (e) => {
        e.preventDefault();
    });

    container.addEventListener('drop', (e) => {
        e.preventDefault();
        const targetLi = e.target.closest('li[data-id]');
        if (!targetLi || !dragged || targetLi === dragged) return;

        const rect = targetLi.getBoundingClientRect();
        const offset = e.clientY - rect.top;

        if (offset < rect.height / 3) {
            targetLi.parentNode.insertBefore(dragged, targetLi);
        } else if (offset > rect.height * 2 / 3) {
            targetLi.parentNode.insertBefore(dragged, targetLi.nextSibling);
        } else {
            let childUl = targetLi.querySelector(':scope > ul');
            if (!childUl) {
                childUl = document.createElement('ul');
                childUl.className = 'ml-4 border-l border-dashed border-gray-200 dark:border-gray-700 pl-3 mt-1 space-y-1';
                targetLi.appendChild(childUl);
            }
            childUl.appendChild(dragged);
        }

        if (saveBtn) {
            saveBtn.disabled = false;
        }
    });
}

