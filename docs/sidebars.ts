import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  tutorialSidebar: [
    'index',
    {
      type: 'category',
      label: 'Getting Started',
      collapsed: false,
      items: ['getting-started'],
    },
    {
      type: 'category',
      label: 'Core Concepts',
      collapsed: false,
      items: ['canonicalization', 'fingerprints'],
    },
    {
      type: 'category',
      label: 'Project',
      items: ['benchmarks', 'using-with-ai'],
    },
  ],
};

export default sidebars;
