// Navigasyon çubuğunun kaydırma ile renk değiştirmesi
window.addEventListener('scroll', function () {
    const navbar = document.querySelector('.navbar');
    if (window.scrollY > 60) {
        navbar.classList.add('scrolled');
    } else {
        navbar.classList.remove('scrolled');
    }
});

// Mobil menü toggle
document.getElementById('mobile-menu').addEventListener('click', function () {
    const navLinks = document.querySelector('.nav-links');
    this.classList.toggle('active');
    navLinks.classList.toggle('active');
});

// Scroll Reveal Efekti
const sections = document.querySelectorAll('.section.scroll-reveal');

const revealSection = function (entries, observer) {
    entries.forEach(entry => {
        if (!entry.isIntersecting) return;

        entry.target.classList.add('is-visible');

        const animatedElements = entry.target.querySelectorAll('.fade-in-up, .slide-in-left, .slide-in-up, .slide-in-right, .pop-in, .zoom-in');

        animatedElements.forEach((el) => {
            const delayClass = Array.from(el.classList).find(cls => cls.startsWith('delay-'));
            let delay = 0;
            if (delayClass) {
                delay = parseInt(delayClass.replace('delay-', '')) / 1000;
            }

            setTimeout(() => {
                el.classList.add('is-visible');
            }, delay * 1000);
        });

        observer.unobserve(entry.target);
    });
};

const sectionObserver = new IntersectionObserver(revealSection, {
    root: null,
    threshold: 0.1,
});

sections.forEach(section => {
    sectionObserver.observe(section);
});

// --- Modal Pencere İşlevselliği ---
const loginRegisterPromptModal = document.getElementById('loginRegisterPromptModal'); // "Şikayet Gönder" için modal
const complaintModal = document.getElementById('complaintModal'); // Şikayet detayı modalı

// Close butonları (modalları kapatmak için)
document.querySelectorAll('.modal .close-button, .modal .modal-close-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        if (loginRegisterPromptModal) {
            loginRegisterPromptModal.style.display = 'none';
        }
        if (complaintModal) {
            complaintModal.style.display = 'none';
        }
    });
});

// Modal dışına tıklayınca kapatma
window.addEventListener('click', function (event) {
    if (event.target == loginRegisterPromptModal) {
        loginRegisterPromptModal.style.display = 'none';
    } else if (event.target == complaintModal && complaintModal) {
        complaintModal.style.display = 'none';
    }
});

// Anasayfadaki "Şikayet Gönder" butonları için modal açma mantığı
[submitComplaintNav, heroSubmitBtn].forEach(button => {
    if (button) {
        button.addEventListener('click', (e) => {
            e.preventDefault(); // Varsayılan link davranışını engelle

            // BURADA GERÇEK KİMLİK DOĞRULAMA KONTROLÜ YAPILMALI
            // Bu kontrol, sunucudan (örneğin bir API endpoint'i) kullanıcının oturum açıp açmadığını sorgulamalıdır.
            // Şimdilik varsayılan olarak kullanıcı giriş yapmamış kabul edelim.
            const isUserLoggedIn = false; // Gerçekte: fetch('/api/Auth/IsLoggedIn').then(res => res.json()).then(data => data.isLoggedIn);

            if (!isUserLoggedIn) {
                // Kullanıcı giriş yapmamışsa, popup modalı aç
                if (loginRegisterPromptModal) {
                    loginRegisterPromptModal.style.display = 'block';
                }
            } else {
                // Kullanıcı giriş yapmışsa, şikayet gönderme formunun bulunduğu sayfaya yönlendir
                alert("Kullanıcı giriş yaptı! Şikayet gönderme formu buraya gelecek.");
                // Örnek: window.location.href = "/Home/SubmitComplaint";
            }
        });
    }
});


// Form gönderildiğinde wobble animasyonu (Sadece İletişim formu)
document.querySelectorAll('.contact-form').forEach(form => {
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        this.classList.add('wobble-animation');
        this.addEventListener('animationend', () => {
            this.classList.remove('wobble-animation');
            // Gerçek uygulamada burada AJAX ile form gönderimi ve sunucu tarafı kontrolü yapılacaktır.
            alert('Mesajınız başarıyla gönderildi!');
            this.reset();
        }, { once: true });
    });
});

// Particles.js konfigürasyonu
particlesJS('particles-js', {
    "particles": {
        "number": {
            "value": 70,
            "density": {
                "enable": true,
                "value_area": 1000
            }
        },
        "color": {
            "value": ["#1A535C", "#4ECDC4", "#EAEFF2"] /* Koyu Mavi, Turkuaz, Açık Gri */
        },
        "shape": {
            "type": "circle",
            "stroke": {
                "width": 0,
                "color": "#000000"
            },
            "polygon": {
                "nb_sides": 5
            }
        },
        "opacity": {
            "value": 0.6,
            "random": true,
            "anim": {
                "enable": true,
                "speed": 0.6,
                "opacity_min": 0.1,
                "sync": false
            }
        },
        "size": {
            "value": 2.8,
            "random": true,
            "anim": {
                "enable": true,
                "speed": 6,
                "size_min": 0.3,
                "sync": false
            }
        },
        "line_linked": {
            "enable": true,
            "distance": 130,
            "color": "#AABCCF", /* Yumuşak gri-mavi çizgiler */
            "opacity": 0.4,
            "width": 1
        },
        "move": {
            "enable": true,
            "speed": 1.5,
            "direction": "none",
            "random": true,
            "straight": false,
            "out_mode": "out",
            "bounce": false,
            "attract": {
                "enable": false,
                "rotateX": 600,
                "rotateY": 1200
            }
        }
    },
    "interactivity": {
        "detect_on": "canvas",
        "events": {
            "onhover": {
                "enable": true,
                "mode": "grab"
            },
            "onclick": {
                "enable": true,
                "mode": "push"
            },
            "resize": true
        },
        "modes": {
            "grab": {
                "distance": 160,
                "line_linked": {
                    "opacity": 0.7
                }
            },
            "bubble": {
                "distance": 250,
                "size": 25,
                "duration": 2,
                "opacity": 5,
                "speed": 3
            },
            "repulse": {
                "distance": 100,
                "duration": 0.4
            },
            "push": {
                "particles_nb": 2
            },
            "remove": {
                "particles_nb": 1
            }
        }
    },
    "retina_detect": true
});