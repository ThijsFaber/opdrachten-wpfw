const pokemonImage = document.getElementById("pokemon-image");
const rerollButton = document.getElementById("reroll-pokemon");

const POKEMON_COUNT = 1025;

async function randomPokemon() {
  const randomNumber = Math.floor(Math.random() * POKEMON_COUNT) + 1;
  const isShiny = Math.random() < 0.1;

  try {
    const response = await fetch(
      `https://pokeapi.co/api/v2/pokemon/${randomNumber}`,
    );

    if (!response.ok) {
      throw new Error("Could not fetch Pokémon");
    }

    const pokemon = await response.json();

    pokemonImage.src = isShiny
      ? pokemon.sprites.other["official-artwork"].front_shiny
      : pokemon.sprites.other["official-artwork"].front_default;
    pokemonImage.alt = isShiny ? `Shiny ${pokemon.name}` : pokemon.name;
  } catch (error) {
    console.error(error);
  }
}

randomPokemon();

rerollButton.addEventListener("click", randomPokemon);
