---
title: "Shell basics: exercises"
lang: en
track: linux
kind: module
doc: exercises
slug: 03-shell-basics
part: 1
tags: [linux, bash, shell, redirection, pipeline, quoting, exit-status]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Shell basics: exercises

🌐 [Русский](exercises.ru.md) · **English**

> Try on your own first, in an empty directory under `~`.

## 1. 🟢 Split the streams

Run `ls /etc/hostname nope` with output into `ok.txt` and errors into `bad.txt`, then print the status and both files.

<details><summary>Hint</summary>

Read `$?` right after the command.

</details>

<details><summary>Solution</summary>

```bash
ls /etc/hostname nope > ok.txt 2> bad.txt
echo "status=$?"
cat ok.txt bad.txt
```

```text
status=2
/etc/hostname
ls: cannot access 'nope': No such file or directory
```

Status 2 from `ls` means serious trouble, such as being unable to access a command-line argument.

</details>

## 2. 🟡 Top words

Get the two most frequent words of a text with their counts, in one pipeline.

<details><summary>Hint</summary>

`tr`, `sort`, `uniq -c`.

</details>

<details><summary>Solution</summary>

```bash
echo 'to be or not to be to' | tr ' ' '\n' | sort | uniq -c | sort -rn | head -n 2
```

```text
      3 to
      2 be
```

`uniq` counts adjacent duplicates only, hence the first `sort`.

</details>

## 3. 🟡 Don't lose the failure

Show that a pipeline with a failing first command "succeeds", then fix it.

<details><summary>Hint</summary>

`pipefail`.

</details>

<details><summary>Solution</summary>

```bash
cat missing 2>/dev/null | wc -l; echo "status=$?"
set -o pipefail
cat missing 2>/dev/null | wc -l; echo "status=$?"
```

```text
0
status=0
0
status=1
```

`wc` prints 0 both times; only the pipeline status differs.

</details>

## 4. 🔴 Files with spaces

Count the lines of every `.txt` file whose names contain spaces, without breaking the names apart.

<details><summary>Hint</summary>

Loop over a pattern and quote.

</details>

<details><summary>Solution</summary>

```bash
printf 'a\nb\n' > 'first file.txt'
printf 'c\n' > 'second file.txt'
for f in *.txt; do
    printf '%s: %s\n' "$f" "$(wc -l < "$f")"
done
```

```text
first file.txt: 2
second file.txt: 1
```

Pattern results are not split into words, and the quoted `"$f"` keeps the name whole.

</details>
