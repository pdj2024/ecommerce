/*
Template Name: ShopGrids - Bootstrap 5 eCommerce HTML Template.
Author: GrayGrids
*/

(function () {
    //===== Prealoder

    //window.onload = function () {
    //    window.setTimeout(fadeout, 500);
    //}

    function fadeout() {
        document.querySelector('.preloader').style.opacity = '0';
        document.querySelector('.preloader').style.display = 'none';
    }


    /*=====================================
    Sticky
    ======================================= */
    window.onscroll = function () {
        var header_navbar = document.querySelector(".navbar");
        //var sticky = header_navbar.offsetTop;

        // show or hide the back-top-top button
        var backToTo = document.querySelector(".scroll-top");

        if (backToTo == null) return;
        if (header_navbar == null) return;

        if (document.body.scrollTop > 50 || document.documentElement.scrollTop > 50) {
            backToTo.style.display = "flex";
            header_navbar.style.boxShadow = "#23232324 5px 10px 20px -8px";
        } else {
            backToTo.style.display = "none";
            header_navbar.style.boxShadow = "unset";
        }
    };


    //===== mobile-menu-btn
    let navbarToggler = document.querySelector(".mobile-menu-btn");
    if (navbarToggler != null) {
        navbarToggler.addEventListener('click', function () {
            navbarToggler.classList.toggle("active");
        });
    }


})();