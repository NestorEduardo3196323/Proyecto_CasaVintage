// Comportamientos del catalogo: hero que rota, filtros y hover que cambia las fotos solo.

(function () {
    "use strict";

    // Si el usuario puede vender (Vendedor). El Administrador ve el catalogo pero no agrega al carrito.
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

    // Boton "ojito": oculta/muestra los productos agotados (stock 0). La preferencia se guarda en el
    // navegador (localStorage), asi sigue activa entre ventas y al ir a otra seccion y volver; solo
    // cambia cuando se vuelve a pulsar el boton. La clase va en el contenedor de la cuadricula, que
    // se conserva al reconstruir las tarjetas con el buscador, asi tambien oculta los nuevos agotados.
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
                texto.textContent = ocultar ? "Mostrar agotados" : "Ocultar agotados";
            }
            boton.title = ocultar ? "Mostrar los productos sin stock" : "Ocultar los productos sin stock";
        }

        aplicar(localStorage.getItem(CLAVE) === "1");

        boton.addEventListener("click", function () {
            var ocultar = localStorage.getItem(CLAVE) !== "1";
            localStorage.setItem(CLAVE, ocultar ? "1" : "0");
            aplicar(ocultar);
        });
    }

    // "Agregar al carrito" (AJAX). Delegado en el documento porque las tarjetas se reconstruyen
    // al buscar. Envia el token anti-forgery y actualiza el contador del icono + un aviso (toast).
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
                        window.mostrarToast(res.mensaje || "Agregado al carrito.", res.exito ? "ok" : "error");
                    }
                })
                .catch(function () {
                    if (window.mostrarToast) {
                        window.mostrarToast("No se pudo agregar. Intenta de nuevo.", "error");
                    }
                })
                .finally(function () { boton.disabled = false; });
        });
    }

    // Galeria del detalle: cambio MANUAL de foto con flechas y miniaturas (no automatico).
    // Las imagenes estan apiladas y se funden (crossfade) al cambiar.
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

    // Hero: muestra una pieza destacada a la vez y va cambiando sola cada pocos segundos.
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

        // Puntos de navegacion (uno por pieza).
        if (dotsCont) {
            slides.forEach(function (_, i) {
                var punto = document.createElement("button");
                punto.type = "button";
                punto.className = "hero-dot" + (i === 0 ? " activo" : "");
                punto.setAttribute("aria-label", "Pieza " + (i + 1));
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

    // Buscador en vivo (AJAX): texto + categoria + epoca + estado. Pide al servidor los productos
    // que coinciden (handler ?handler=Buscar que devuelve JSON) y reconstruye la cuadricula.
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
                .catch(function () { /* si la peticion falla, se deja la cuadricula como esta */ });
        }

        // La busqueda por texto se retrasa un poco (debounce) para no pedir en cada tecla.
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

    // Escapa texto para insertarlo con seguridad dentro del HTML de una tarjeta.
    function esc(valor) {
        return String(valor == null ? "" : valor)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    var ICONO_SIN_FOTO = '<span class="producto-sin-foto" aria-hidden="true"><svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><polyline points="21 15 16 10 5 21"/></svg></span>';

    // Construye el HTML de una tarjeta a partir de un producto (respuesta JSON del buscador).
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

        var conteo = fotos.length > 1 ? '<span class="producto-conteo-fotos">' + fotos.length + ' fotos</span>' : "";
        var stock = p.disponibilidad
            ? '<span class="badge-estado es-activo">' + p.stock + ' en stock</span>'
            : '<span class="badge-estado es-inactivo">Sin stock</span>';
        var botonAgregar = "";
        if (puedeAgregar) {
            botonAgregar = p.disponibilidad
                ? '<button type="button" class="btn-vintage btn-sm" data-agregar="' + p.idProducto + '">Agregar</button>'
                : '<button type="button" class="btn-vintage btn-sm" disabled>Agotado</button>';
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
            '<a href="/Catalogo/Detalle/' + p.idProducto + '" class="btn-vintage-outline btn-sm">Detalles</a>' +
            botonAgregar +
            '</div></div></div></article>';
    }

    // Hover: al pasar el mouse sobre una tarjeta con varias fotos, las va mostrando en secuencia.
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
