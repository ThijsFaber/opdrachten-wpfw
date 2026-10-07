const container = document.querySelector("#blog-container");

function blogToHTML(blog) {
  return `
    <article class="card">
      <img
        class="post-image"
        src="${blog.image.src}"
        alt="${blog.image.alt}"
      />

      <div class="post-header">
        <h2 class="post-title">${blog.title}</h2>

        <p class="post-meta">
          ${blog.meta}
          <time class="post-date" datetime="${blog.date}">
            ${new Date(blog.date).toLocaleDateString("nl-NL", {
              day: "numeric",
              month: "long",
              year: "numeric",
            })}
          </time>
        </p>
      </div>

      <div class="post-body">
        <p>${blog.content}</p>

        <div class="post-actions">
          <button type="button" class="button">
            ${blog.button}
          </button>
        </div>
      </div>
    </article>
  `;
}

fetch("json/blog.json")
  .then((response) => {
    if (!response.ok) {
      throw new Error(`HTTP error: ${response.status}`);
    }

    return response.json();
  })
  .then((blogs) => {
    container.innerHTML = blogs.map((blog) => blogToHTML(blog)).join("");
  })
  .catch((error) => {
    console.error("Blog laden mislukt:", error);

    container.innerHTML = `
      <p>De blog kon niet worden geladen.</p>
    `;
  });


