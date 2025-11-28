# Sail Documentation

This folder contains the documentation for Sail, a modern API Gateway built on YARP.

## For GitHub Pages

This documentation is designed to be published as a GitHub Pages site. The main entry point is `index.md`.

## Structure

- `index.md` - Main documentation homepage
- `articles/` - Detailed guides and documentation articles
  - `getting-started.md` - Quick start guide
  - `architecture.md` - Architecture overview
  - (Add more articles as needed)
- `_config.yml` - Jekyll configuration for GitHub Pages

## Building Locally

To preview the documentation locally:

```bash
# Install Jekyll (if not already installed)
gem install bundler jekyll

# Serve the site locally
cd docs
jekyll serve
```

Visit `http://localhost:4000` to preview the site.

## Publishing to GitHub Pages

1. Ensure your repository settings have GitHub Pages enabled
2. Set the source to the `main` branch and `/docs` folder
3. Push your changes to the `main` branch
4. GitHub will automatically build and deploy the site

## Contributing

When adding new documentation:

1. Create markdown files in the appropriate directory
2. Add YAML front matter at the top of each file:
   ```yaml
   ---
   title: Your Page Title
   ---
   ```
3. Update navigation in `_config.yml` if needed
4. Use relative links between pages

## Style Guide

- Use clear, concise language
- Include code examples where appropriate
- Use proper markdown formatting
- Add screenshots for UI-related documentation
- Keep code blocks properly formatted with language specifiers

## More Information

For more information about Sail, visit the [main repository](https://github.com/lqlive/sail).

