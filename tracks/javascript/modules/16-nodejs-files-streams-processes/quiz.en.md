---
title: "Quiz: Node.js — files, streams, processes"
lang: en
track: javascript
kind: module
doc: quiz
slug: 16-nodejs-files-streams-processes
part: 2
tags: [fs, streams, pipeline, child-process, process-env]
status: draft
updated: 2026-11-23
---

# Quiz: Node.js — files, streams, processes

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which `fs` style is recommended for new code?

- A) Synchronous `*Sync`
- B) Callbacks
- C) `node:fs/promises`
- D) Only streams

<details><summary>Answer</summary>

**C.** Promises fit `async`/`await`; synchronous calls block the event loop.
→ [Basic theory](basics.en.md#three-styles-of-fs)

</details>

## 2. 🟢 What does `fsp.readFile(path)` return without an encoding?

- A) A string
- B) A `Buffer`
- C) An array of strings
- D) `undefined`

<details><summary>Answer</summary>

**B.** Without an encoding, bytes are returned.
→ [Basic theory](basics.en.md#reading-and-writing)

</details>

## 3. 🟢 What happens with `process.env.NEW_VAR = 42`?

- A) The number `42`
- B) The string `'42'`
- C) `undefined`
- D) An error

<details><summary>Answer</summary>

**B.** `process.env` values are always strings.
→ [Basic theory](basics.en.md#the-process-the-environment-arguments)

</details>

## 4. 🟢 How do you build a path correctly?

- A) `dir + '/' + name`
- B) `path.join(dir, name)`
- C) `` `${dir}\${name}` ``
- D) `dir.concat(name)`

<details><summary>Answer</summary>

**B.** `path.join` accounts for OS separators and normalizes the path.
→ [Basic theory](basics.en.md#paths-the-path-module)

</details>

## 5. 🟡 How is `pipeline` better than `readable.pipe(writable)`?

- A) Faster
- B) It forwards errors, destroys all links and returns a promise
- C) It works only with files
- D) Not at all

<details><summary>Answer</summary>

**B.** `pipe` does not forward errors and does not close the destination.
→ [Advanced theory](advanced.en.md#pipeline)

</details>

## 6. 🟡 What does `false` from `writable.write(chunk)` mean?

- A) The write failed
- B) The internal buffer is full: wait for `drain`
- C) The stream is closed
- D) Data is lost

<details><summary>Answer</summary>

**B.** This is the backpressure signal.
→ [Advanced theory](advanced.en.md#backpressure)

</details>

## 7. 🟡 How many bytes does the string `'héllo'` take in `Buffer.from`?

- A) 5
- B) 6
- C) 10
- D) 4

<details><summary>Answer</summary>

**B.** `é` is two bytes in UTF-8: the string `length` is 5, the buffer length is 6.
→ [Advanced theory](advanced.en.md#buffer)

</details>

## 8. 🟡 Which function should you use to run a program with user-supplied arguments?

- A) `exec('prog ' + arg)`
- B) `execFile('prog', [arg])`
- C) `eval`
- D) `exec` with quotes

<details><summary>Answer</summary>

**B.** Without a shell an argument cannot add extra commands.
→ [Advanced theory](advanced.en.md#child-processes)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) `process.exitCode = 1` does not interrupt the program
- [ ] B) `process.exit()` can cut off unfinished operations
- [ ] C) `parseArgs` returns numbers for `--count=3`
- [ ] D) An ES module has `import.meta.dirname`

<details><summary>Answer</summary>

**A, B, D.** C is wrong: values are strings.
→ [Basic theory](basics.en.md#exit-codes)

</details>

## 10. 🔴 Find the mistake

> "`if (fs.existsSync(f)) data = fs.readFileSync(f)` is a safe way to read a file that may be missing."

<details><summary>Answer</summary>

Between the check and the read the file may disappear (a race). It is more reliable to read and catch `ENOENT`.
→ [Gotcha 1](expert.en.md#1-existssync-and-a-race)

</details>

## 11. 🔴 What happens on an error in a `Readable` with no `error` handler?

- A) The error is swallowed
- B) The process crashes with `Unhandled 'error' event`
- C) The stream closes silently
- D) The error becomes a warning

<details><summary>Answer</summary>

**B.** An `error` event with no listener throws an exception.
→ [Gotcha 4](expert.en.md#4-a-stream-without-an-error-handler)

</details>

## 12. 🔴 What happens when you start a nonexistent program with `spawn`?

- A) `ENOENT` in the `error` event
- B) `0` in the `exit` event
- C) An exception when calling `spawn`
- D) `ERR_UNKNOWN`

<details><summary>Answer</summary>

**A.** The error arrives as an `error` event (in the experiment `spawn error ENOENT`); without a handler the process crashes.
→ [Advanced theory](advanced.en.md#child-processes)

</details>

## 13. 🔴 Open question

How would you process a multi-gigabyte file (counting lines with a given word) and compress the result without exhausting memory?

<details><summary>Key points of a good answer</summary>

- Read with a stream (`createReadStream`), get lines via `readline`; do not use `readFile`.
- Filter in a `Transform` (objectMode) and write the result through compression (`createGzip`) into a file.
- Connect the links with `pipeline` (errors, backpressure, closing).
- Handle `error` and exit codes (`process.exitCode`), avoid synchronous calls.

</details>

→ [Advanced theory](advanced.en.md#streams)
