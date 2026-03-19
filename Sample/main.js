document.addEventListener('DOMContentLoaded', () => {
    // Navigation Scroll Effect
    const header = document.querySelector('header');
    window.addEventListener('scroll', () => {
        if (window.scrollY > 50) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    });


    // Dynamic Temple Status
    function updateTempleStatus() {
        const now = new Date();
        const hour = now.getHours();
        const minute = now.getMinutes();
        const currentTime = hour * 60 + minute;

        const morningOpen = 4 * 60 + 30; // 04:30 AM
        const morningClose = 13 * 60;    // 01:00 PM
        const eveningOpen = 16 * 60 + 15; // 04:15 PM
        const eveningClose = 21 * 60;    // 09:00 PM

        const statusElement = document.querySelector('.status-dot');
        const statusText = document.querySelector('.status-text');

        if (statusElement && statusText) {
            if (currentTime >= morningOpen && currentTime < morningClose) {
                // Currently in Morning Session
                statusElement.style.backgroundColor = '#2ecc71'; // Green
                statusText.textContent = 'OPEN | Morning Darsana until 01:00 PM';
            } else if (currentTime >= eveningOpen && currentTime < eveningClose) {
                // Currently in Evening Session
                statusElement.style.backgroundColor = '#2ecc71'; // Green
                statusText.textContent = 'OPEN | Evening Darsana until 09:00 PM';
            } else {
                // Currently Closed
                statusElement.style.backgroundColor = '#e74c3c'; // Red
                if (currentTime < morningOpen) {
                    statusText.textContent = 'CLOSED | Opens at 04:30 AM for Mangala Arati';
                } else if (currentTime < eveningOpen) {
                    statusText.textContent = 'CLOSED | Opens at 04:15 PM for Evening Darsana';
                } else {
                    statusText.textContent = 'CLOSED | Opens Tomorrow at 04:30 AM';
                }
            }
        }
    }

    updateTempleStatus();
    setInterval(updateTempleStatus, 60000); // Update every minute

    // Generic Carousel Logic
    function setupCarousel(wrapperSelector, sliderSelector, prevBtnSelector, nextBtnSelector, desktopVisible = 3) {
        const slider = document.querySelector(sliderSelector);
        const cards = slider ? slider.children : [];
        const prevBtn = document.querySelector(prevBtnSelector);
        const nextBtn = document.querySelector(nextBtnSelector);
        
        if (slider && cards.length > 0) {
            let counter = 0;

            const updateSlider = () => {
                const screenWidth = window.innerWidth;
                let visibleCards = desktopVisible;
                if (screenWidth <= 600) visibleCards = 1;
                else if (screenWidth <= 1000 && desktopVisible > 1) visibleCards = 2; // For courses etc.
                else if (desktopVisible === 1) visibleCards = 1;
                
                const movePercent = 100 / visibleCards;
                slider.style.transform = `translateX(${-counter * movePercent}%)`;
            };

            nextBtn.addEventListener('click', () => {
                const screenWidth = window.innerWidth;
                let visibleCards = desktopVisible;
                if (screenWidth <= 600) visibleCards = 1;
                else if (screenWidth <= 1000 && desktopVisible > 1) visibleCards = 2;
                else if (desktopVisible === 1) visibleCards = 1;

                if (counter >= cards.length - visibleCards) {
                    counter = 0; // Reset to start
                } else {
                    counter++;
                }
                updateSlider();
            });

            prevBtn.addEventListener('click', () => {
                const screenWidth = window.innerWidth;
                let visibleCards = desktopVisible;
                if (screenWidth <= 600) visibleCards = 1;
                else if (screenWidth <= 1000 && desktopVisible > 1) visibleCards = 2;
                else if (desktopVisible === 1) visibleCards = 1;

                if (counter <= 0) {
                    counter = cards.length - visibleCards; // Go to end
                } else {
                    counter--;
                }
                updateSlider();
            });

            window.addEventListener('resize', updateSlider);
            // Initial call
            updateSlider();
        }
    }

    // Initialize Carousels
    setupCarousel('.events-wrapper', '.events-slider', '.events-section .btn-prev', '.events-section .btn-next', 1);
    setupCarousel('.courses-wrapper', '.courses-slider', '.courses-container .btn-prev', '.courses-container .btn-next', 3);

    // Login Module Logic
    const injectLoginModal = () => {
        const modalHTML = `
            <div class="modal-overlay" id="loginModal">
                <div class="login-modal">
                    <span class="modal-close">&times;</span>
                    
                    <!-- Login View -->
                    <div id="loginView">
                        <h2>Member Login</h2>
                        <form id="loginForm">
                            <div class="form-group">
                                <label>Membership Number / Email</label>
                                <input type="text" placeholder="BOM-XXXXX or your email" required>
                            </div>
                            <div class="form-group">
                                <label>Password</label>
                                <input type="password" placeholder="••••••••" required>
                            </div>
                            <button type="submit" class="login-btn">Sign In</button>
                        </form>
                        <p style="text-align: center; margin-top: 20px; font-size: 0.85rem; color: #666;">
                            Don't have an account? <a href="#" id="toRegister" style="color: var(--primary-purple); font-weight: 600;">Register Now</a>
                        </p>
                    </div>

                    <!-- Registration View (New User) -->
                    <div id="registerView" style="display: none;">
                        <h2>New User Registration</h2>
                        <form id="registerForm">
                            <div class="form-group">
                                <label>1) Name</label>
                                <input type="text" placeholder="Your Full Name" required>
                            </div>
                            <div class="form-group">
                                <label>2) Date of Birth (DOB)</label>
                                <input type="date" required>
                            </div>
                            <div class="form-group">
                                <label>3) Address</label>
                                <textarea placeholder="Your Permanent Address" style="width: 100%; padding: 12px; border: 1px solid #ddd; border-radius: 8px; font-family: inherit;" rows="3" required></textarea>
                            </div>
                            <div class="form-group">
                                <label>4) Working Nature</label>
                                <input type="text" placeholder="Occupation / Student / Retired" required>
                            </div>
                            <button type="submit" class="login-btn">Register Now</button>
                        </form>
                        <p style="text-align: center; margin-top: 20px; font-size: 0.85rem; color: #666;">
                            Already have an account? <a href="#" id="toLogin" style="color: var(--primary-purple); font-weight: 600;">Back to Login</a>
                        </p>
                    </div>
                </div>
            </div>
        `;
        document.body.insertAdjacentHTML('beforeend', modalHTML);
    };

    injectLoginModal();

    const modal = document.getElementById('loginModal');
    const loginTriggers = document.querySelectorAll('.login-trigger');
    const closeBtn = document.querySelector('.modal-close');
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');
    const loginView = document.getElementById('loginView');
    const registerView = document.getElementById('registerView');
    const toRegister = document.getElementById('toRegister');
    const toLogin = document.getElementById('toLogin');

    const toggleView = (viewToShow) => {
        loginView.style.display = viewToShow === 'login' ? 'block' : 'none';
        registerView.style.display = viewToShow === 'register' ? 'block' : 'none';
    };

    toRegister.addEventListener('click', (e) => {
        e.preventDefault();
        toggleView('register');
    });

    toLogin.addEventListener('click', (e) => {
        e.preventDefault();
        toggleView('login');
    });

    loginTriggers.forEach(trigger => {
        trigger.addEventListener('click', () => {
            toggleView('login'); // Always start with login
            modal.classList.add('active');
            document.body.style.overflow = 'hidden';
        });
    });

    const closeModal = () => {
        modal.classList.remove('active');
        document.body.style.overflow = 'auto';
    };

    closeBtn.addEventListener('click', closeModal);
    modal.addEventListener('click', (e) => {
        if (e.target === modal) closeModal();
    });

    loginForm.addEventListener('submit', (e) => {
        e.preventDefault();
        const btn = loginForm.querySelector('.login-btn');
        btn.textContent = 'Authenticating...';
        btn.disabled = true;

        setTimeout(() => {
            alert('Login Successful! Welcome to ISKCON Dhule Portal.');
            closeModal();
            btn.textContent = 'Sign In';
            btn.disabled = false;
            
            // Mock UI change after login
            loginTriggers.forEach(trigger => {
                trigger.innerHTML = 'Account 👤';
                trigger.classList.remove('btn-gold');
                trigger.style.backgroundColor = '#f0f0f0';
                trigger.style.color = 'var(--primary-purple)';
            });
        }, 1500);
    });

    registerForm.addEventListener('submit', (e) => {
        e.preventDefault();
        const btn = registerForm.querySelector('.login-btn');
        btn.textContent = 'Processing...';
        btn.disabled = true;

        setTimeout(() => {
            alert('Registration Successful! Your membership request has been submitted to ISKCON Dhule office.');
            closeModal();
            btn.textContent = 'Register Now';
            btn.disabled = false;
        }, 1500);
    });
});
