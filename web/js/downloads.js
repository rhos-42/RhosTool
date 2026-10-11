(function () {
  const c = window.RHOS_CONFIG || {};
  const groups = window.RHOS_RESOURCE_GROUPS || [];
  const urls = c.resourceUrls || {};
  const folders = c.resourceFolderUrls || {};

  const dlMain = document.getElementById("dl-main");
  const dlLabel = document.getElementById("dl-label");
  const dlSetup = document.getElementById("dl-setup");
  const dlSetupLabel = document.getElementById("dl-setup-label");
  const dlVer = document.getElementById("dl-version");
  const dlReleases = document.getElementById("dl-releases");
  const dlGh = document.getElementById("dl-gh");
  const catNav = document.getElementById("cat-nav");
  const sections = document.getElementById("resource-sections");
  const statsEl = document.getElementById("res-stats");
  const hubEl = document.getElementById("mf-hub");

  if (c.downloadUrl && dlMain) dlMain.href = c.downloadUrl;
  if (c.downloadLabel && dlLabel) dlLabel.textContent = c.downloadLabel;
  if (c.installUrl && dlSetup) dlSetup.href = c.installUrl;
  if (c.installLabel && dlSetupLabel) dlSetupLabel.textContent = c.installLabel;
  if (c.downloadVersion && dlVer) dlVer.textContent = c.downloadVersion;
  if (c.releasesUrl && dlReleases) dlReleases.href = c.releasesUrl;
  if (c.githubUrl && dlGh) dlGh.href = c.githubUrl;
  if (hubEl && c.mediafireHub) {
    hubEl.href = c.mediafireHub;
    hubEl.hidden = false;
  }

  function isFolder(u) {
    return /mediafire\.com\/folder\//i.test(u || "");
  }

  function resolveUrl(id) {
    const u = (urls[id] || "").trim();
    if (!u) return "";
    const folder = u.match(/mediafire\.com\/folder\/([a-zA-Z0-9]+)(?:\/([^/?#]+))?/i);
    if (folder) {
      const key = folder[1];
      const name = (folder[2] || "folder").replace(/\/$/, "");
      return `https://www.mediafire.com/folder/${key}/${name}`;
    }
    const file = u.match(/mediafire\.com\/(?:file|download|view|file_premium)\/([a-zA-Z0-9]+)(?:\/([^/?#]+))?/i);
    if (file) {
      const key = file[1];
      const name = (file[2] || "download").replace(/\/$/, "");
      return `https://www.mediafire.com/file/${key}/${name}/file`;
    }
    return u;
  }

  let ready = 0;
  let total = 0;

  groups.forEach((group) => {
    const chip = document.createElement("a");
    chip.className = "cat-chip";
    chip.href = "#" + group.id;
    chip.textContent = group.title;
    catNav.appendChild(chip);

    const section = document.createElement("section");
    section.className = "res-section";
    section.id = group.id;

    const folderUrl = (folders[group.id] || "").trim();
    const gReady = group.items.filter((i) => resolveUrl(i.id)).length;

    const head = document.createElement("div");
    head.className = "res-head";
    head.innerHTML =
      `<div>` +
      `<h2>${group.title}</h2>` +
      `<p class="dl-meta">${group.blurb || ""}</p>` +
      `</div>` +
      `<span class="badge ${gReady ? "badge-ok" : "badge-wait"}">${gReady}/${group.items.length}</span>`;
    section.appendChild(head);

    if (folderUrl) {
      const place = document.createElement("div");
      place.className = "mf-place";
      place.innerHTML =
        `<span class="dl-meta">Lieu MediaFire</span>` +
        `<a class="btn btn-primary" href="${folderUrl}" target="_blank" rel="noopener noreferrer">Ouvrir le dossier</a>`;
      section.appendChild(place);
    }

    const ul = document.createElement("ul");
    ul.className = "dl-list";

    group.items.forEach((item) => {
      total += 1;
      const url = resolveUrl(item.id);
      if (url) ready += 1;
      const folder = isFolder(url);

      const li = document.createElement("li");
      li.innerHTML =
        `<div>` +
        `<strong>${item.title}</strong>` +
        `<div class="dl-meta">${url ? (folder ? "Dossier MediaFire" : "Fichier MediaFire") : "Bientôt disponible"}</div>` +
        `</div>`;

      const a = document.createElement(url ? "a" : "span");
      a.className = "btn" + (url ? " btn-primary" : " btn-disabled");
      a.textContent = url ? (folder ? "Ouvrir" : "Télécharger") : "Bientôt";
      if (url) {
        a.href = url;
        a.target = "_blank";
        a.rel = "noopener noreferrer";
      }
      li.appendChild(a);
      ul.appendChild(li);
    });

    section.appendChild(ul);
    sections.appendChild(section);
  });

  if (statsEl) {
    statsEl.className = "badge " + (ready ? "badge-ok" : "badge-wait");
    statsEl.textContent = `${ready} lieu(x) MediaFire · ${total} ressources`;
  }
})();
