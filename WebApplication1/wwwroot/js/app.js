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

// Scroll Reveal Efekti (Mevcut hali korunuyor)
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
const loginModal = document.getElementById('loginModal');
const registerModal = document.getElementById('registerModal');
const complaintModal = document.getElementById('complaintModal'); // Şikayet detayı modalı

const loginNavBtn = document.getElementById('loginNavBtn');
const registerNavBtn = document.getElementById('registerNavBtn');
const submitComplaintNav = document.getElementById('submitComplaintNav');

const showRegisterFromLogin = document.getElementById('showRegisterFromLogin');
const showLoginFromRegister = document.getElementById('showLoginFromRegister');

// Close butonları
document.querySelectorAll('.modal .close-button, .modal .modal-close-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        loginModal.style.display = 'none';
        registerModal.style.display = 'none';
        complaintModal.style.display = 'none';
    });
});

// Modal dışına tıklayınca kapatma
window.addEventListener('click', function (event) {
    if (event.target == loginModal) {
        loginModal.style.display = 'none';
    } else if (event.target == registerModal) {
        registerModal.style.display = 'none';
    } else if (event.target == complaintModal) {
        complaintModal.style.display = 'none';
    }
});


// Navbar Giriş Yap butonu
if (loginNavBtn) {
    loginNavBtn.addEventListener('click', (e) => {
        e.preventDefault();
        registerModal.style.display = 'none'; // Diğer modalı kapat
        loginModal.style.display = 'block'; // Giriş modalını aç
    });
}

// Navbar Kayıt Ol butonu
if (registerNavBtn) {
    registerNavBtn.addEventListener('click', (e) => {
        e.preventDefault();
        loginModal.style.display = 'none'; // Diğer modalı kapat
        registerModal.style.display = 'block'; // Kayıt modalını aç
    });
}

// Giriş modalından Kayıt ol'a geçiş
if (showRegisterFromLogin) {
    showRegisterFromLogin.addEventListener('click', (e) => {
        e.preventDefault();
        loginModal.style.display = 'none';
        registerModal.style.display = 'block';
    });
}

// Kayıt modalından Giriş yap'a geçiş
if (showLoginFromRegister) {
    showLoginFromRegister.addEventListener('click', (e) => {
        e.preventDefault();
        registerModal.style.display = 'none';
        loginModal.style.display = 'block';
    });
}

// Şikayet Gönder butonu (Navbar) - Kullanıcı girişi gerektirecek
if (submitComplaintNav) {
    submitComplaintNav.addEventListener('click', (e) => {
        e.preventDefault();
        // Varsayılan olarak kullanıcı giriş yapmamış kabul edelim.
        // Gerçek uygulamada burada bir kimlik doğrulama kontrolü yapılmalı (örn. isUserLoggedIn flag).
        const isUserLoggedIn = false; // BU DEĞİŞKENİ GERÇEK UYGULAMANIZDA SUNUCU TARAFINDAN KONTROL EDİN

        if (!isUserLoggedIn) {
            alert("Şikayet göndermek için lütfen giriş yapın veya kayıt olun.");
            loginModal.style.display = 'block'; // Giriş modalını aç
        } else {
            // Kullanıcı giriş yapmışsa, şikayet gönderme sayfasına yönlendir veya formu göster
            // Şu an için bir yere yönlendirmiyoruz, bu sadece bir UI/UX akış örneği.
            alert("Şikayet gönderme formu buraya gelecek. (Kullanıcı giriş yapmış varsayılıyor)");
            // window.location.href = "#sikayet-gonder"; // Eğer aynı sayfadaki bölüme yönlendirecekse
        }
    });
}

// Ana sayfadaki "Şikayetini Şimdi Gönder" butonu (Hero Section)
const heroSubmitBtn = document.querySelector('.hero-buttons .btn-primary-filled');
if (heroSubmitBtn) {
    heroSubmitBtn.addEventListener('click', (e) => {
        e.preventDefault();
        const isUserLoggedIn = false; // BU DEĞİŞKENİ GERÇEK UYGULAMANIZDA SUNUCU TARAFINDAN KONTROL EDİN

        if (!isUserLoggedIn) {
            alert("Şikayet göndermek için lütfen giriş yapın veya kayıt olun.");
            loginModal.style.display = 'block'; // Giriş modalını aç
        } else {
            alert("Şikayet gönderme formu buraya gelecek. (Kullanıcı giriş yapmış varsayılıyor)");
        }
    });
}

// Form gönderildiğinde wobble animasyonu (Login, Register, Complaint, Contact)
document.querySelectorAll('.auth-form, .complaint-form, .contact-form').forEach(form => {
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        this.classList.add('wobble-animation');
        this.addEventListener('animationend', () => {
            this.classList.remove('wobble-animation');
            // Gerçek uygulamada burada AJAX ile form gönderimi ve sunucu tarafı kontrolü yapılacaktır.
            if (this.classList.contains('auth-form')) {
                // Giriş veya kayıt başarılıysa modalı kapat, kullanıcıyı giriş yapmış say
                alert(this.querySelector('h3').textContent + ' Başarılı! Hoş geldiniz.');
                loginModal.style.display = 'none';
                registerModal.style.display = 'none';
                // Kullanıcıyı giriş yapmış duruma getir (frontend için dummy)
                // Gerçekte: localStorage.setItem('isLoggedIn', 'true');
            } else if (this.classList.contains('complaint-form')) {
                alert('Şikayetiniz başarıyla gönderildi! Kayıtlarınızı ve cevapları takip etmek için "Şikayetlerim" bölümünü ziyaret edebilirsiniz.');
            } else if (this.classList.contains('contact-form')) {
                alert('Mesajınız başarıyla gönderildi!');
            }
            this.reset();
        }, { once: true });
    });
});

// Şikayet Detay Modalını Açma (Mevcut işlevsellik)
const viewDetailsButtons = document.querySelectorAll('.view-details-btn');

// Örnek şikayet verileri (Gerçek uygulamada API'den çekilir)
const dummyComplaints = {
    1: {
        title: "Yol Bakım Talebi - Çukur",
        status: "Beklemede",
        date: "15.07.2025",
        location: "Cumhuriyet Cad. No: 12",
        detail: "Cumhuriyet Caddesi üzerindeki 12 numaralı binanın önünde oluşan büyük çukur, özellikle yağmurlu havalarda araçlar için ciddi bir tehlike oluşturmaktadır. Lastik patlamalarına ve araç hasarlarına yol açmaktadır. Acil müdahale rica olunur.",
        response: "Şikayetiniz ilgili birime iletilmiştir. En kısa sürede inceleme yapılacaktır."
    },
    2: {
        title: "Çevre Temizliği - Çöp Kutusu",
        status: "Çözüldü",
        date: "01.07.2025",
        location: "Park Sok. No: 5",
        detail: "Park Sokak'taki park alanında bulunan çöp kutusu uzun süredir boşaltılmamış, çöp etrafa yayılmıştır. Bu durum hem kötü kokuya hem de çevre kirliliğine neden olmaktadır. Temizlik ekiplerinin müdahalesi gerekmektedir.",
        response: "Şikayetiniz üzerine ekiplerimiz bölgeye yönlendirilmiş ve çöp kutusu boşaltılarak çevre temizliği yapılmıştır. Duyarlılığınız için teşekkür ederiz."
    },
    3: {
        title: "Gürültü Kirliliği - İnşaat",
        status: "İşlemde",
        date: "10.07.2025",
        location: "Barış Mah. İnşaat Alanı",
        detail: "Gece geç saatlere kadar süren inşaat gürültüsü, mahalle sakinlerinin uyku düzenini olumsuz etkilemektedir. Gerekli denetimlerin yapılmasını rica ederiz.",
        response: "Şikayetiniz ilgili zabıta birimine iletilmiştir. İnşaat sahasında denetim yapılarak gerekli uyarılar ve işlemler başlatılacaktır."
    }
};

viewDetailsButtons.forEach(button => {
    button.addEventListener('click', function () {
        const complaintId = this.dataset.complaintId;
        const complaint = dummyComplaints[complaintId];

        if (complaint) {
            document.getElementById('modalComplaintTitle').textContent = complaint.title;
            document.getElementById('modalComplaintStatus').textContent = complaint.status;
            // Statü rengini de güncelle
            const statusBadge = document.getElementById('modalComplaintStatus');
            statusBadge.className = 'status-badge'; // Önceki sınıfları temizle
            if (complaint.status === 'Beklemede') {
                statusBadge.classList.add('status-pending');
            } else if (complaint.status === 'İşlemde') {
                statusBadge.classList.add('status-in-progress');
            } else if (complaint.status === 'Çözüldü') {
                statusBadge.classList.add('status-resolved');
            }
            document.getElementById('modalComplaintDate').textContent = complaint.date;
            document.getElementById('modalComplaintLocation').textContent = complaint.location;
            document.getElementById('modalComplaintDetail').textContent = complaint.detail;
            document.getElementById('modalComplaintResponse').textContent = complaint.response || "Henüz bir cevap bulunmamaktadır.";
            complaintModal.style.display = 'block';
        }
    });
});


// Particles.js konfigürasyonu (Yeni renk paleti ile güncellendi)
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