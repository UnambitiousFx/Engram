import type {ReactNode} from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';
import HomepageFeatures from '@site/src/components/HomepageFeatures';

import styles from './index.module.css';

const QUICK_START = `using UnambitiousFx.Engram;

var a = """{ "currency": "EUR", "amount": 12.50 }""";
var b = """{"amount":12.5,"currency":"EUR"}""";

// 1 · One canonical form
JsonCanonicalizer.Canonicalize(a);
// {"amount":12.5,"currency":"EUR"}

// 2 · One fingerprint
JsonCanonicalizer.Hash(a) == JsonCanonicalizer.Hash(b);
// true

// 3 · The bytes to sign
byte[] payload = JsonCanonicalizer.CanonicalizeToUtf8(a);`;

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={styles.heroBanner}>
      <div className={clsx('container', styles.heroInner)}>
        <div className={styles.heroLeft}>
          <Heading as="h1" className="hero__title" style={{color: 'inherit', margin: 0}}>
            {siteConfig.title}
          </Heading>
          <p className={styles.heroTagline}>{siteConfig.tagline}</p>
          <div className={styles.heroButtons}>
            <Link className="button button--primary button--lg" to="/docs/getting-started">
              Get Started →
            </Link>
            <Link
              className="button button--outline button--lg"
              style={{color: 'inherit', borderColor: 'rgba(255,255,255,0.4)'}}
              href="https://github.com/UnambitiousFx/Engram">
              View on GitHub
            </Link>
          </div>
          <div className={styles.heroInstall}>
            <code>dotnet add package UnambitiousFx.Engram</code>
          </div>
        </div>

        <div className={styles.heroCode}>
          <div className={styles.heroCodeLabel}>Quick start</div>
          <pre>{QUICK_START}</pre>
        </div>
      </div>
    </header>
  );
}

type NextStepItem = {
  emoji: string;
  title: string;
  description: string;
  to: string;
};

const NEXT_STEPS: NextStepItem[] = [
  {
    emoji: '🚀',
    title: 'Getting Started',
    description: 'Install the package and canonicalize your first document.',
    to: '/docs/getting-started',
  },
  {
    emoji: '📐',
    title: 'Canonicalization',
    description: 'What RFC 8785 changes in a JSON text, and what it refuses.',
    to: '/docs/canonicalization',
  },
  {
    emoji: '🧬',
    title: 'Fingerprints',
    description: 'Idempotency keys, change detection, signatures and ETags.',
    to: '/docs/fingerprints',
  },
];

export default function Home(): ReactNode {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={siteConfig.title}
      description="RFC 8785 JSON Canonicalization Scheme for .NET: one deterministic form for any JSON value and a SHA-256 fingerprint, NativeAOT-ready.">
      <HomepageHeader />
      <main>
        <HomepageFeatures />
        <section className={styles.nextSteps}>
          <div className="container">
            <Heading as="h2" className="text--center">Explore the docs</Heading>
            <div className={styles.nextStepsGrid}>
              {NEXT_STEPS.map((item) => (
                <Link key={item.to} to={item.to} style={{textDecoration: 'none'}}>
                  <div
                    style={{
                      background: 'var(--engram-card-bg)',
                      border: '1px solid var(--engram-card-border)',
                      borderRadius: '10px',
                      padding: '1.25rem',
                      height: '100%',
                      transition: 'box-shadow 0.2s, transform 0.2s',
                    }}
                    onMouseEnter={e => {
                      (e.currentTarget as HTMLDivElement).style.transform = 'translateY(-2px)';
                      (e.currentTarget as HTMLDivElement).style.boxShadow = 'var(--engram-card-shadow-hover)';
                    }}
                    onMouseLeave={e => {
                      (e.currentTarget as HTMLDivElement).style.transform = '';
                      (e.currentTarget as HTMLDivElement).style.boxShadow = '';
                    }}>
                    <div style={{fontSize: '1.75rem', marginBottom: '0.5rem'}}>{item.emoji}</div>
                    <Heading as="h3" style={{fontSize: '1rem', marginBottom: '0.4rem'}}>
                      {item.title}
                    </Heading>
                    <p style={{fontSize: '0.875rem', margin: 0, opacity: 0.75}}>{item.description}</p>
                  </div>
                </Link>
              ))}
            </div>
          </div>
        </section>
      </main>
    </Layout>
  );
}
