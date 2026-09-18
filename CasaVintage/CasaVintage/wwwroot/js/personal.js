// Behaviors for the "Company staff" view.
// Progressive: without JS the cards still show and the actions still work (normal POSTs).

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        activarInfo();
        activarDialogoRestablecer();
        activarRevelado();
        activarVistaPreviaFoto();
    });

    // Previews the chosen image before submitting it. Works for one or several fields: each
    // [data-foto-input] input shows its preview in the [data-foto-previa] of its own group.
    function activarVistaPreviaFoto() {
        document.querySelectorAll("[data-foto-input]").forEach(function (input) {
            var grupo = input.closest("[data-foto-grupo]") || input.parentElement;
            var previa = grupo ? grupo.querySelector("[data-foto-previa]") : null;
            if (!previa) {
                return;
            }

            input.addEventListener("change", function () {
                var archivo = input.files && input.files[0];
                if (!archivo) {
                    return;
                }
                var url = URL.createObjectURL(archivo);
                previa.innerHTML = "";
                var img = document.createElement("img");
                img.src = url;
                img.alt = "Preview";
                img.addEventListener("load", function () {
                    URL.revokeObjectURL(url);
                });
                previa.appendChild(img);
            });
        });
    }

    // "Info" panel: expands/collapses each employee's data and actions.
    function activarInfo() {
        document.querySelectorAll("[data-info-toggle]").forEach(function (boton) {
            boton.addEventListener("click", function () {
                var panel = document.getElementById(boton.getAttribute("aria-controls"));
                if (!panel) {
                    return;
                }
                var abierto = boton.getAttribute("aria-expanded") === "true";
                boton.setAttribute("aria-expanded", String(!abierto));
                boton.classList.toggle("abierto", !abierto);
                panel.hidden = abierto;
            });
        });
    }

    // Single dialog to reset a password: a button per card opens it with the id and name.
    function activarDialogoRestablecer() {
        var dialogo = document.getElementById("dlgRestablecer");
        if (!dialogo) {
            return;
        }

        var campoId = dialogo.querySelector("input[name='Restablecer.IdUsuario']");
        var nombre = document.getElementById("dlgRestablecerNombre");
        var campoPass = dialogo.querySelector("input[name='Restablecer.Password']");

        function abrir() {
            if (typeof dialogo.showModal === "function") {
                dialogo.showModal();
            } else {
                dialogo.setAttribute("open", "");
            }
            if (campoPass) {
                campoPass.focus();
            }
        }

        document.querySelectorAll("[data-restablecer]").forEach(function (boton) {
            boton.addEventListener("click", function () {
                if (campoId) {
                    campoId.value = boton.getAttribute("data-id") || "";
                }
                if (nombre) {
                    nombre.textContent = boton.getAttribute("data-nombre") || "Employee";
                }
                abrir();
            });
        });

        dialogo.querySelectorAll("[data-cerrar-dialogo]").forEach(function (boton) {
            boton.addEventListener("click", function () {
                dialogo.close();
            });
        });

        // If the server asked to reopen (validation failed), it is shown with the errors.
        if (window.CasaVintagePersonal && window.CasaVintagePersonal.reabrirRestablecer) {
            abrir();
        }
    }

    // Soft reveal of the cards on scroll. The content is already visible by default:
    // the animation is only applied if there is JS and the user did not ask to reduce motion.
    function activarRevelado() {
        var lista = document.querySelector("[data-reveal]");
        if (!lista || !("IntersectionObserver" in window)) {
            return;
        }
        if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            return;
        }

        var filas = Array.prototype.slice.call(lista.querySelectorAll(".personal-fila"));
        if (!filas.length) {
            return;
        }

        lista.classList.add("con-revelado");
        filas.forEach(function (fila) {
            fila.classList.add("por-revelar");
        });

        function revelar(fila) {
            fila.classList.add("revelada");
        }

        var observador = new IntersectionObserver(function (entradas) {
            entradas.forEach(function (entrada) {
                if (entrada.isIntersecting) {
                    revelar(entrada.target);
                    observador.unobserve(entrada.target);
                }
            });
        }, { threshold: 0.15, rootMargin: "0px 0px -40px 0px" });

        filas.forEach(function (fila) {
            observador.observe(fila);
        });

        // Safeguard: if the observer does not fire (background tab or render without painting),
        // the hidden state is removed so the content is NEVER left invisible.
        // The class is removed (not animated) because transitions do not advance without painting.
        setTimeout(function () {
            filas.forEach(function (fila) {
                fila.classList.remove("por-revelar");
            });
        }, 700);
    }
})();
