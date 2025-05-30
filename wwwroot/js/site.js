document.addEventListener('DOMContentLoaded', () => {
    // Drag-and-drop
    const matchForm = document.getElementById('match-form');
    if (matchForm) {
        const checkButton = document.getElementById('check-match');
        checkButton.addEventListener('click', (e) => {
            e.preventDefault();
            const formData = new FormData(matchForm);
            const selects = matchForm.querySelectorAll('select');
            let valid = true;
            selects.forEach(select => {
                if (!select.value) {
                    valid = false;
                }
            });
            if (!valid) {
                const feedback = document.getElementById('match-feedback');
                feedback.textContent = 'Пожалуйста, выберите ответ для всех пунктов.';
                feedback.classList.remove('hidden', 'text-green-600');
                feedback.classList.add('text-red-600');
                return;
            }
            fetch(matchForm.action, {
                method: 'POST',
                body: formData
            }).then(response => response.text()).then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newContent = doc.querySelector('main').innerHTML;
                document.querySelector('main').innerHTML = newContent;
                const score = doc.querySelector('header p').textContent.match(/\d+/)[0];
                document.querySelector('header p').textContent = `Очки: ${score}`;
            }).catch(error => {
                console.error('Error during drag-and-drop fetch:', error);
                const feedback = document.getElementById('match-feedback');
                feedback.textContent = 'Ошибка при проверке. Попробуйте снова.';
                feedback.classList.remove('hidden', 'text-green-600');
                feedback.classList.add('text-red-600');
            });
        });
    }

    // Sorting
    const sortingForm = document.getElementById('sorting-form');
    if (sortingForm) {
        const checkButton = document.getElementById('check-sorting');
        checkButton.addEventListener('click', (e) => {
            e.preventDefault();
            const formData = new FormData(sortingForm);
            const selects = sortingForm.querySelectorAll('select');
            let valid = true;
            const selectedValues = new Set();
            selects.forEach(select => {
                if (!select.value) {
                    valid = false;
                } else if (selectedValues.has(select.value)) {
                    valid = false;
                } else {
                    selectedValues.add(select.value);
                }
            });
            const feedback = document.getElementById('sorting-feedback');
            if (!valid) {
                feedback.textContent = !selectedValues.size ? 'Пожалуйста, выберите элемент для каждой позиции.' : 'Ошибка: каждый элемент должен быть выбран только один раз.';
                feedback.classList.remove('hidden', 'text-green-600');
                feedback.classList.add('text-red-600');
                return;
            }
            fetch(sortingForm.action, {
                method: 'POST',
                body: formData
            }).then(response => response.text()).then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newContent = doc.querySelector('main').innerHTML;
                document.querySelector('main').innerHTML = newContent;
                const score = doc.querySelector('header p').textContent.match(/\d+/)[0];
                document.querySelector('header p').textContent = `Очки: ${score}`;
            }).catch(error => {
                console.error('Error during sorting fetch:', error);
                const feedback = document.getElementById('sorting-feedback');
                feedback.textContent = 'Ошибка при проверке. Попробуйте снова.';
                feedback.classList.remove('hidden', 'text-green-600');
                feedback.classList.add('text-red-600');
            });
        });
    }

    // True-False
    const trueFalseForm = document.getElementById('true-false-form');
    if (trueFalseForm) {
        const checkButton = document.getElementById('check-true-false');
        checkButton.addEventListener('click', (e) => {
            e.preventDefault();
            const formData = new FormData(trueFalseForm);
            const radios = trueFalseForm.querySelectorAll('input[type="radio"]:checked');
            if (radios.length < trueFalseForm.querySelectorAll('input[type="radio"]').length / 2) {
                const feedback = document.getElementById('true-false-feedback');
                feedback.textContent = 'Пожалуйста, выберите ответ для всех утверждений.';
                feedback.classList.remove('hidden', 'text-green-600');
                feedback.classList.add('text-red-600');
                return;
            }
            fetch(trueFalseForm.action, {
                method: 'POST',
                body: formData
            }).then(response => response.text()).then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newContent = doc.querySelector('main').innerHTML;
                document.querySelector('main').innerHTML = newContent;
                const score = doc.querySelector('header p').textContent.match(/\d+/)[0];
                document.querySelector('header p').textContent = `Очки: ${score}`;
            }).catch(error => {
                console.error('Error during true-false fetch:', error);
                const feedback = document.getElementById('true-false-feedback');
                feedback.textContent = 'Ошибка при проверке. Попробуйте снова.';
                feedback.classList.remove('hidden', 'text-green-600');
                feedback.classList.add('text-red-600');
            });
        });
    }
});