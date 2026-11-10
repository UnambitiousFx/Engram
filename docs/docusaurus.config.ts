import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

const config: Config = {
  title: 'Engram - UnambitiousFx',
  tagline: 'RFC 8785 JSON canonicalization and content fingerprints for .NET',
  favicon: 'img/favicon.ico',

  headTags: [
    {
      tagName: 'link',
      attributes: {
        rel: 'icon',
        type: 'image/svg+xml',
        href: '/img/favicon.svg',
      },
    },
    {
      tagName: 'link',
      attributes: {
        rel: 'apple-touch-icon',
        href: '/img/apple-touch-icon.png',
      },
    },
  ],

  // Future flags, see https://docusaurus.io/docs/api/docusaurus-config#future
  future: {
    v4: true, // Improve compatibility with the upcoming Docusaurus v4
  },

  url: 'https://engram.unambitiousfx.com',
  baseUrl: '/',

  // GitHub pages deployment config.
  organizationName: 'UnambitiousFx',
  projectName: 'Engram',

  plugins: [
    [
      'docusaurus-plugin-llms',
      {
        generateLLMsTxt: true,
        generateLLMsFullTxt: true,
        excludeImports: true,
        removeDuplicateHeadings: true,
        includeOrder: ['index.mdx', 'getting-started.mdx', 'canonicalization.mdx', 'fingerprints.mdx'],
        includeUnmatchedLast: true,
      },
    ],
  ],

  onBrokenLinks: 'throw',

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          editUrl: 'https://github.com/UnambitiousFx/Engram/',
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    image: 'img/unambitiousfx-social-card.png',
    colorMode: {
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'Engram',
      logo: {
        alt: 'UnambitiousFx Engram Logo',
        src: 'img/unambitiousfx-icon-light.svg',
        srcDark: 'img/unambitiousfx-icon-dark.svg',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'tutorialSidebar',
          position: 'left',
          label: 'Docs',
        },
        {
          type: 'doc',
          docId: 'canonicalization',
          position: 'left',
          label: 'Canonicalization',
        },
        {
          type: 'doc',
          docId: 'fingerprints',
          position: 'left',
          label: 'Fingerprints',
        },
        {
          href: 'https://www.nuget.org/packages/UnambitiousFx.Engram',
          label: 'NuGet',
          position: 'right',
        },
        {
          href: 'https://github.com/UnambitiousFx/Engram',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {label: 'Getting Started', to: '/docs/getting-started'},
            {label: 'Canonicalization', to: '/docs/canonicalization'},
            {label: 'Fingerprints', to: '/docs/fingerprints'},
          ],
        },
        {
          title: 'More',
          items: [
            {label: 'GitHub', href: 'https://github.com/UnambitiousFx/Engram'},
            {label: 'NuGet', href: 'https://www.nuget.org/packages/UnambitiousFx.Engram'},
            {label: 'RFC 8785', href: 'https://www.rfc-editor.org/rfc/rfc8785'},
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} UnambitiousFx. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
