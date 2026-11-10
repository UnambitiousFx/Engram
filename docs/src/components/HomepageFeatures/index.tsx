import type {CSSProperties, ReactNode} from 'react';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

type FeatureItem = {
  icon: string;
  accent: string;
  title: string;
  description: ReactNode;
};

const FeatureList: FeatureItem[] = [
  {
    icon: '📐',
    accent: '#512bd4',
    title: 'RFC 8785, byte for byte',
    description: (
      <>
        Members sorted by UTF-16 code units, minimal escaping, ECMAScript number
        formatting. Tested against the RFC and its reference implementation.
      </>
    ),
  },
  {
    icon: '🧬',
    accent: '#2563eb',
    title: 'Content fingerprints',
    description: (
      <>
        <code>Hash</code> gives the SHA-256 of the canonical form: the same data
        has the same fingerprint, however it was written.
      </>
    ),
  },
  {
    icon: '🛡️',
    accent: '#d97706',
    title: 'Strict by design',
    description: (
      <>
        Duplicate members, out-of-range numbers and lone surrogates are refused
        instead of producing a form no other implementation would reproduce.
      </>
    ),
  },
  {
    icon: '🔒',
    accent: '#2e8555',
    title: 'NativeAOT-ready',
    description: (
      <>
        No dependency beyond the BCL. Typed values go through their
        source-generated <code>JsonTypeInfo&lt;T&gt;</code>, never reflection.
      </>
    ),
  },
];

function Feature({icon, accent, title, description}: FeatureItem) {
  return (
    <div className={styles.featureCard} style={{'--card-accent': accent} as CSSProperties}>
      <div className={styles.featureIconWrap}>
        <span className={styles.featureIcon}>{icon}</span>
      </div>
      <Heading as="h3" className={styles.featureTitle}>{title}</Heading>
      <p className={styles.featureDescription}>{description}</p>
    </div>
  );
}

export default function HomepageFeatures(): ReactNode {
  return (
    <section className={styles.features}>
      <div className="container">
        <Heading as="h2" className={styles.sectionHeading}>Why Engram?</Heading>
        <div className={styles.featuresGrid}>
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
