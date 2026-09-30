const pokemonImage = document.getElementById("pokemon-image");
const rerollButton = document.getElementById("reroll-pokemon");

const POKEMON_COUNT = 1025;

async function randomPokemon() {
  rerollButton.disabled = true;
  pokemonImage.alt = "Pokémon wordt geladen...";

  try {
    const randomNumber = Math.floor(Math.random() * POKEMON_COUNT) + 1;
    const isShiny = Math.random() < 0.1;

    const response = await fetch(
      `https://pokeapi.co/api/v2/pokemon/${randomNumber}`,
    );

    if (!response.ok) {
      throw new Error(`PokéAPI gaf een fout (${response.status}).`);
    }

    const pokemon = await response.json();

    pokemonImage.src = isShiny
      ? pokemon.sprites.other["official-artwork"].front_shiny
      : pokemon.sprites.other["official-artwork"].front_default;
    pokemonImage.alt = isShiny ? `Shiny ${pokemon.name}` : pokemon.name;
  } catch (error) {
    console.error("Pokémon ophalen mislukt:", error);

    pokemonImage.removeAttribute("src");
    pokemonImage.alt = "Pokémon kon niet worden geladen";
    pokemonImage.title =
      "Kon Pokémon niet laden. Klik op ↻ om opnieuw te proberen";
  } finally {
    rerollButton.disabled = false;
  }
}

randomPokemon();

rerollButton.addEventListener("click", randomPokemon);
