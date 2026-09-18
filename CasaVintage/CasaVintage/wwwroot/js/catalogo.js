// Catalog behaviors: rotating hero, filters and hover that cycles the photos on its own.

(function () {
    "use strict";

    // Whether the user can sell (Salesperson). The Administrator sees the catalog but cannot add to cart.
    var puedeAgregar = true;

    document.addEventListener("DOMContentLoaded", function () {
        var grid = document.querySelector("[data-catalogo]");
        puedeAgregar = !grid || grid.dataset.puedeAgregar !== "false";
        activarHero();
        activarBusqueda();
        activarHoverFotos();
        activarGaleria();
        activarAgregarCarrito();
        activarOcultarAgotados();
    });

    // "Eye" button: hides/shows out-of-stock products (stock 0). The preference is saved in the
    // browser (localStorage), so it stays active between sales and when leaving to another section and
    // coming back; it only changes when the button is pressed again. The class goes on the grid
    // container, which is preserved when the cards are rebuilt by the search, so it also hides new
    // out-of-stock items.
    function activarOcultarAgotados() {
        var boton = document.querySelector("[data-toggle-agotados]");
        var grid = document.querySelector("[data-catalogo]");
        if (!boton || !grid) {
            return;
        }
        var CLAVE = "catalogoOcultarAgotados";
        var texto = boton.querySelector("[data-texto-agotados]");

        function aplicar(ocultar) {
            grid.classList.toggle("ocultar-agotados", ocultar);
            boton.classList.toggle("activo", ocultar);
            boton.setAttribute("aria-pressed", ocultar ? "true" : "false");
            if (texto) {
                texto.textContent = ocultar ? "Show out of stock" : "Hide out of stock";
            }
            boton.title = ocultar ? "Show out-of-stock products" : "Hide out-of-stock products";
        }

        aplicar(localStorage.getItem(CLAVE) === "1");

        boton.addEventListener("click", function () {
            var ocultar = localStorage.getItem(CLAVE) !== "1";
            localStorage.setItem(CLAVE, ocultar ? "1" : "0");
            aplicar(ocultar);
        });
    }

    // "Add to cart" (AJAX). Delegated on the document because the cards are rebuilt when searching.
    // Sends the anti-forgery token and updates the icon counter + a notice (toast).
    function activarAgregarCarrito() {
        document.addEventListener("click", function (evento) {
            var boton = evento.target.closest("[data-agregar]");
            if (!boton) {
                return;
            }
            var id = boton.getAttribute("data-agregar");
            var token = document.querySelector('input[name="__RequestVerificationToken"]');
            boton.disabled = true;

            fetch("/Catalogo/Index?handler=Agregar", {
                method: "POST",
                headers: {
                    "RequestVerificationToken": token ? token.value : "",
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                body: "id=" + encodeURIComponent(id)
            })
                .then(function (r) { return r.json(); })
                .then(function (res) {
                    if (window.actualizarCarritoBadge) {
                        window.actualizarCarritoBadge(res.totalProductos);
                    }
                    if (window.mostrarToast) {
                        window.mostrarToast(res.mensaje || "Added to cart.", res.exito ? "ok" : "error");
                    }
                })
                .catch(function () {
                    if (window.mostrarToast) {
                        window.mostrarToast("Could not add. Try again.", "error");
                    }
                })
                .finally(function () { boton.disabled = false; });
        });
    }

    // Detail gallery: MANUAL photo change with arrows and thumbnails (not automatic).
    // The images are stacked and crossfade when switching.
    function activarGaleria() {
        var galeria = document.querySelector("[data-galeria]");
        if (!galeria) {
            return;
        }
        var slides = Array.prototype.slice.call(galeria.querySelectorAll("[data-galeria-slide]"));
        var thumbs = Array.prototype.slice.call(galeria.querySelectorAll("[data-galeria-thumb]"));
        if (slides.length <= 1) {
            return;
        }

        var actual = 0;

        function mostrar(indice) {
            actual = (indice + slides.length) % slides.length;
            slides.forEach(function (s, i) {
                s.classList.toggle("activa", i === actual);
            });
            thumbs.forEach(function (t, i) {
                t.classList.toggle("activa", i === actual);
            });
        }

        thumbs.forEach(function (thumb, i) {
            thumb.addEventListener("click", function () {
                mostrar(i);
            });
        });

        var prev = galeria.querySelector("[data-galeria-prev]");
        var next = galeria.querySelector("[data-galeria-next]");
        if (prev) {
            prev.addEventListener("click", function () {
                mostrar(actual - 1);
            });
        }
        if (next) {
            next.addEventListener("click", function () {
                mostrar(actual + 1);
            });
        }
    }

    // Hero: shows one featured piece at a time and cycles on its own every few seconds.
    function activarHero() {
        var hero = document.querySelector("[data-hero]");
        if (!hero) {
            return;
        }
        var slides = Array.prototype.slice.call(hero.querySelectorAll("[data-hero-slide]"));
        if (slides.length <= 1) {
            return;
        }

        var dotsCont = hero.querySelector("[data-hero-dots]");
        var actual = 0;
        var puntos = [];

        function mostrar(indice) {
            slides[actual].classList.remove("activa");
            if (puntos[actual]) {
                puntos[actual].classList.remove("activo");
            }
            actual = (indice + slides.length) % slides.length;
            slides[actual].classList.add("activa");
            if (puntos[actual]) {
                puntos[actual].classList.add("activo");
            }
        }

        // Navigation dots (one per piece).
        if (dotsCont) {
            slides.forEach(function (_, i) {
                var punto = document.createElement("button");
                punto.type = "button";
                punto.className = "hero-dot" + (i === 0 ? " activo" : "");
                punto.setAttribute("aria-label", "Piece " + (i + 1));
                punto.addEventListener("click", function () {
                    mostrar(i);
                    reiniciar();
                });
                dotsCont.appendChild(punto);
                puntos.push(punto);
            });
        }

        var temporizador = null;
        function reiniciar() {
            if (temporizador) {
                window.clearInterval(temporizador);
            }
            temporizador = window.setInterval(function () {
                mostrar(actual + 1);
            }, 4500);
        }
        reiniciar();
    }

    // Live search (AJAX): text + category + era + condition. Asks the server for the matching
    // products (handler ?handler=Buscar that returns JSON) and rebuilds the grid.
    function activarBusqueda() {
        var grid = document.querySelector("[data-catalogo]");
        if (!grid) {
            return;
        }
        var vacio = document.querySelector("[data-catalogo-vacio]");
        var input = document.querySelector("[data-catalogo-buscar]");
        var pills = document.querySelector("[data-filtro-categoria]");
        var selEpoca = document.querySelector("[data-filtro-epoca]");
        var selEstado = document.querySelector("[data-filtro-estado]");

        var categoria = "";
        var temporizador = null;

        function aplicar() {
            var params = new URLSearchParams();
            params.set("handler", "Buscar");
            if (input && input.value.trim()) {
                params.set("q", input.value.trim());
            }
            if (categoria) {
                params.set("categoria", categoria);
            }
            if (selEpoca && selEpoca.value) {
                params.set("epoca", selEpoca.value);
            }
            if (selEstado && selEstado.value) {
                params.set("estado", selEstado.value);
            }

            fetch(window.location.pathname + "?" + params.toString(), { headers: { "X-Requested-With": "fetch" } })
                .then(function (r) { return r.ok ? r.json() : []; })
                .then(function (productos) {
                    grid.innerHTML = productos.map(crearTarjeta).join("");
                    if (vacio) {
                        vacio.hidden = productos.length !== 0;
                    }
                    activarHoverFotos();
                })
                .catch(function () { /* if the request fails, the grid is left as it is */ });
        }

        // The text search is delayed a little (debounce) so it does not query on every keystroke.
        if (input) {
            input.addEventListener("input", function () {
                if (temporizador) {
                    window.clearTimeout(temporizador);
                }
                temporizador = window.setTimeout(aplicar, 250);
            });
        }
        if (pills) {
            pills.querySelectorAll(".pill").forEach(function (pill) {
                pill.addEventListener("click", function () {
                    pills.querySelectorAll(".pill").forEach(function (p) {
                        p.classList.remove("activo");
                    });
                    pill.classList.add("activo");
                    categoria = pill.getAttribute("data-cat") || "";
                    aplicar();
                });
            });
        }
        if (selEpoca) {
            selEpoca.addEventListener("change", aplicar);
        }
        if (selEstado) {
            selEstado.addEventListener("change", aplicar);
        }
    }

    // Escapes text to safely insert it inside a card's HTML.
    function esc(valor) {
        return String(valor == null ? "" : valor)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    var ICONO_SIN_FOTO = '<span class="producto-sin-foto" aria-hidden="true"><svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><polyline points="21 15 16 10 5 21"/></svg></span>';

    // Builds a card's HTML from a product (JSON response of the search).
    function crearTarjeta(p) {
        var fotos = [p.foto1, p.foto2, p.foto3].filter(Boolean);
        var desc = p.descripcion && p.descripcion.length > 90
            ? p.descripcion.slice(0, 90).replace(/\s+$/, "") + "…"
            : (p.descripcion || "");
        var precio = Number(p.precio).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

        var imagenes = fotos.length > 0
            ? fotos.map(function (f, i) {
                return '<img src="' + esc(f) + '" alt="' + esc(p.nombre) + '" class="producto-img' + (i === 0 ? " activa" : "") + '" loading="lazy" />';
            }).join("")
            : ICONO_SIN_FOTO;

        var conteo = fotos.length > 1 ? '<span class="producto-conteo-fotos">' + fotos.length + ' photos</span>' : "";
        var stock = p.disponibilidad
            ? '<span class="badge-estado es-activo">' + p.stock + ' in stock</span>'
            : '<span class="badge-estado es-inactivo">Out of stock</span>';
        var botonAgregar = "";
        if (puedeAgregar) {
            botonAgregar = p.disponibilidad
                ? '<button type="button" class="btn-vintage btn-sm" data-agregar="' + p.idProducto + '">Add</button>'
                : '<button type="button" class="btn-vintage btn-sm" disabled>Out of stock</button>';
        }

        return '<article class="producto-card ' + (p.disponibilidad ? "" : "agotado") + '"' +
            ' data-categoria="' + esc(p.categoria) + '" data-epoca="' + esc(p.epoca) + '" data-estado="' + esc(p.estado) + '">' +
            '<div class="producto-imagen" data-fotos>' + imagenes +
            '<span class="producto-categoria">' + esc(p.categoria) + '</span>' + conteo + '</div>' +
            '<div class="producto-cuerpo">' +
            '<div class="producto-meta">' + esc(p.epoca) + ' &middot; ' + esc(p.estado) + '</div>' +
            '<h3 class="producto-nombre">' + esc(p.nombre) + '</h3>' +
            '<p class="producto-desc">' + esc(desc) + '</p>' +
            '<div class="producto-datos"><span class="sku-chip">' + esc(p.sku) + '</span>' + stock + '</div>' +
            '<div class="producto-pie">' +
            '<span class="producto-precio">$' + precio + '</span>' +
            '<div class="producto-acciones">' +
            '<a href="/Catalogo/Detalle/' + p.idProducto + '" class="btn-vintage-outline btn-sm">Details</a>' +
            botonAgregar +
            '</div></div></div></article>';
    }

    // Hover: hovering over a card with several photos cycles through them in sequence.
    function activarHoverFotos() {
        document.querySelectorAll(".producto-imagen[data-fotos]").forEach(function (contenedor) {
            var fotos = Array.prototype.slice.call(contenedor.querySelectorAll(".producto-img"));
            if (fotos.length <= 1) {
                return;
            }

            var indice = 0;
            var temporizador = null;

            function mostrar(i) {
                fotos[indice].classList.remove("activa");
                indice = (i + fotos.length) % fotos.length;
                fotos[indice].classList.add("activa");
            }

            contenedor.addEventListener("mouseenter", function () {
                temporizador = window.setInterval(function () {
                    mostrar(indice + 1);
                }, 850);
            });

            contenedor.addEventListener("mouseleave", function () {
                if (temporizador) {
                    window.clearInterval(temporizador);
                    temporizador = null;
                }
                mostrar(0);
            });
        });
    }
})();
