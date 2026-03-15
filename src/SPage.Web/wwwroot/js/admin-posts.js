document.addEventListener('DOMContentLoaded', () => {
    initPostSlug();
    initMetaCounters();
    initFeaturedImageUpload();
    initPostEditor();
});

function slugify(text) {
    if (!text) return '';
    text = text.replace(/[đĐ]/g, 'd');
    text = text.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    text = text.toLowerCase();
    text = text.replace(/[^a-z0-9\s-]/g, '-');
    text = text.replace(/[\s-]+/g, '-');
    return text.replace(/^-+|-+$/g, '');
}

function initPostSlug() {
    const titleEl = document.getElementById('post-title');
    const slugEl = document.getElementById('post-slug');

    if (titleEl && slugEl) {
        let slugEdited = false;
        slugEl.addEventListener('input', () => { slugEdited = true; });
        titleEl.addEventListener('input', () => {
            if (!slugEdited) {
                slugEl.value = slugify(titleEl.value);
            }
        });
        slugEl.addEventListener('blur', () => {
            if (slugEl.value.trim()) {
                slugEl.value = slugify(slugEl.value);
            }
        });
    } else if (slugEl) {
        slugEl.addEventListener('blur', () => {
            if (slugEl.value.trim()) {
                slugEl.value = slugify(slugEl.value);
            }
        });
    }
}

function initCounter(inputId, counterId, max) {
    const el = document.getElementById(inputId);
    const counter = document.getElementById(counterId);
    if (!el || !counter) return;

    const update = () => {
        const len = el.value.length;
        counter.textContent = `${len}/${max}`;
        counter.className = len > max * 0.9
            ? 'text-xs text-orange-500'
            : 'text-xs text-slate-400';
    };

    el.addEventListener('input', update);
    update();
}

function initMetaCounters() {
    initCounter('meta-title', 'meta-title-count', 70);
    initCounter('meta-desc', 'meta-desc-count', 160);
}

async function uploadImageFromInput(fileInputId, targetInputId, previewId = null) {
    const fileInput = document.getElementById(fileInputId);
    const targetInput = document.getElementById(targetInputId);
    if (!fileInput || !targetInput) return;

    const file = fileInput.files[0];
    if (!file) return;

    const uploadUrl = fileInput.getAttribute('data-upload-url');
    if (!uploadUrl) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const formData = new FormData();
    formData.append('upload', file);

    const headers = {};
    if (token) {
        headers['RequestVerificationToken'] = token;
    }

    try {
        const response = await fetch(uploadUrl, {
            method: 'POST',
            body: formData,
            headers: headers
        });

        if (!response.ok) {
            alert('Upload hình thất bại');
            return;
        }

        const data = await response.json();
        targetInput.value = data.url;

        if (previewId) {
            const previewEl = document.getElementById(previewId);
            if (previewEl) {
                previewEl.src = data.url;
                previewEl.classList.remove('hidden');
            }
        }
    } catch (e) {
        console.error(e);
        alert('Không thể upload hình');
    } finally {
        fileInput.value = '';
    }
}

function attachPreviewListener(inputId, previewId) {
    const inputEl = document.getElementById(inputId);
    const previewEl = document.getElementById(previewId);

    if (inputEl && previewEl) {
        if (inputEl.value) {
            previewEl.src = inputEl.value;
            previewEl.classList.remove('hidden');
        } else {
            previewEl.classList.add('hidden');
        }

        inputEl.addEventListener('input', () => {
            if (inputEl.value) {
                previewEl.src = inputEl.value;
                previewEl.classList.remove('hidden');
            } else {
                previewEl.classList.add('hidden');
            }
        });
    }
}

function initFeaturedImageUpload() {
    const fileInput = document.getElementById('featured-image-file');
    attachPreviewListener('featured-image-url', 'featured-image-preview');

    // Auto-fill Alt Text if empty
    const altInput = document.getElementById('FeaturedImageAlt');
    const titleInput = document.getElementById('Name') || document.getElementById('Title');

    const updateAltText = () => {
        if (altInput && titleInput && !altInput.value.trim()) {
            altInput.value = titleInput.value.trim();
        }
    };

    if (titleInput && altInput) {
        titleInput.addEventListener('change', updateAltText);
    }

    if (!fileInput) return;

    fileInput.addEventListener('change', () => {
        uploadImageFromInput('featured-image-file', 'featured-image-url', 'featured-image-preview');
        updateAltText();
    });
}

function initPostEditor() {
    const editorEl = document.querySelector('#content-editor');
    if (!editorEl || typeof tinymce === 'undefined') return;

    const uploadUrl = editorEl.getAttribute('data-upload-url');
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    tinymce.init({
        selector: '#content-editor',
        height: 500,
        menubar: true,
        plugins: [
            'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
            'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
            'insertdatetime', 'media', 'table', 'code', 'help', 'wordcount'
        ],
        toolbar: 'undo redo | blocks | ' +
            'bold italic forecolor | alignleft aligncenter ' +
            'alignright alignjustify | bullist numlist outdent indent | ' +
            'image media table | code fullscreen preview | removeformat | help',
        images_upload_handler: uploadUrl ? (blobInfo, progress) => new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.withCredentials = false;
            xhr.open('POST', uploadUrl);

            if (token) {
                xhr.setRequestHeader('RequestVerificationToken', token);
            }

            xhr.upload.onprogress = (e) => {
                progress(e.loaded / e.total * 100);
            };

            xhr.onload = () => {
                if (xhr.status === 403) {
                    reject({ message: 'HTTP Error: ' + xhr.status, remove: true });
                    return;
                }
                if (xhr.status < 200 || xhr.status >= 300) {
                    reject('HTTP Error: ' + xhr.status);
                    return;
                }
                const json = JSON.parse(xhr.responseText);
                if (!json || typeof json.url != 'string') {
                    reject('Invalid JSON: ' + xhr.responseText);
                    return;
                }
                resolve(json.url);
            };

            xhr.onerror = () => {
                reject('Image upload failed due to a XHR Transport error. Code: ' + xhr.status);
            };

            const formData = new FormData();
            formData.append('upload', blobInfo.blob(), blobInfo.filename());

            xhr.send(formData);
        }) : undefined,
        content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px }',
        setup: function (editor) {
            editor.on('change', function () {
                editor.save(); // Ensures the underlying textarea gets updated
            });
        }
    });
}
