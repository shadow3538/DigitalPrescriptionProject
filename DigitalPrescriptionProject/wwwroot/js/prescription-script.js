const toggleBtn = document.getElementById('toggle-btn');
const sidebar = document.getElementById('sidebar');

// Hamburger Menu বাটনে ক্লিক করলে সাইডবার টগল হবে
toggleBtn.addEventListener('click', () => {
    sidebar.classList.toggle('expanded');
});