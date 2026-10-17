const form = document.querySelector("#contact-form");
const status = document.querySelector("#form-status");
const velden = [
  { id: "naam", boodschap: "Vul minimaal 2 tekens in." },
  { id: "email", boodschap: "Vul een geldig e-mailadres in." },
  { id: "bericht", boodschap: "Schrijf minimaal 10 tekens." },
];

function valideerVeld(veld) {
  const input = document.querySelector(`#${veld.id}`);
  const foutmelding = document.querySelector(`#${veld.id}-error`);

  // Catch whitespace-only input, which passes required/minlength
  const min = Number(input.getAttribute("minlength")) || 0;
  input.setCustomValidity(input.value.trim().length < min ? veld.boodschap : "");

  const geldig = input.checkValidity();
  input.setAttribute("aria-invalid", String(!geldig));
  foutmelding.textContent = geldig ? "" : veld.boodschap;
  return geldig;
}

// Re-validate while typing, but only once a field has been flagged
velden.forEach((veld) => {
  const input = document.querySelector(`#${veld.id}`);
  input.addEventListener("input", () => {
    if (input.getAttribute("aria-invalid") === "true") valideerVeld(veld);
  });
});

form.addEventListener("submit", (event) => {
  event.preventDefault();
  const alleGeldig = velden.map(valideerVeld).every(Boolean);

  if (!alleGeldig) {
    status.textContent = "Er zijn nog fouten in het formulier.";
    form.querySelector('[aria-invalid="true"]').focus();
    return;
  }

  status.textContent = "Bericht verzonden! Bedankt.";
  form.reset();
});