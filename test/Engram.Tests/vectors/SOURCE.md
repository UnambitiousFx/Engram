# RFC 8785 test vectors

- `official/`: copied from https://github.com/cyberphone/json-canonicalization (`testdata/input`, `testdata/output`)
  at commit 19d51d7fe467d4706a3ff08adf8a748f29fc21e0, under the Apache License 2.0 (`official/LICENSE`). Unmodified.
- `numbers.txt`: RFC 8785 Appendix B, "Number Serialization Samples", one `ieee-hex,expected` pair per line;
  `expected` is `error` for NaN and infinities.
