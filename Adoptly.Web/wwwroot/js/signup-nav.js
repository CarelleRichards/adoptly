const adopterProperties = document.querySelector("#adopterProperties");
const shelterProperties = document.querySelector("#shelterProperties");
const generalProperties = document.querySelector("#generalProperties");

const adopterRadio = document.querySelector("#Adopter");
const shelterRadio = document.querySelector("#Shelter");

function signupRadio() {
    if (adopterRadio.checked) {
        shelterProperties.classList.add("hidden");
        adopterProperties.classList.remove("hidden");
        generalProperties.classList.remove("hidden");
        adopterProperties.classList.add("flex");
        generalProperties.classList.add("flex");
        shelterProperties.classList.remove("flex");

    } else if (shelterRadio.checked) {
        adopterProperties.classList.add("hidden");
        shelterProperties.classList.remove("hidden");
        generalProperties.classList.remove("hidden");
        shelterProperties.classList.add("flex");
        generalProperties.classList.add("flex");
        adopterProperties.classList.remove("flex");
    }
}

signupRadio();