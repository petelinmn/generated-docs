---
title: "Quiz: files and permissions"
lang: en
track: linux
kind: module
doc: quiz
slug: 04-files-and-permissions
part: 1
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Quiz: files and permissions

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is `rw-r-----` in numbers?

- A) `644`
- B) `640`
- C) `460`
- D) `600`

<details><summary>Answer</summary>

**B.** 4+2, 4, 0.

→ [Basics](basics.en.md)

</details>

## 2. 🟢 What does `x` on a directory allow?

- A) Listing names
- B) Passing through and accessing entries by name
- C) Running every file inside
- D) Deleting the directory

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Who can give a file to another user with `chown`?

- A) The owner
- B) Only root
- C) Any group member
- D) Anyone with `w`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Which command adds execute permission for the owner only?

- A) `chmod +x f`
- B) `chmod u+x f`
- C) `chmod 111 f`
- D) `chown u+x f`

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 5. 🟡 With `umask 027`, a new ordinary file gets

- A) `750`
- B) `640`
- C) `027`
- D) `644`

<details><summary>Answer</summary>

**B.** `666` without the bits of `027`.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 A file with mode `060` is owned by `ann`, who is also in its group. Can `ann` read it?

- A) Yes, through the group
- B) No
- C) Only with `sudo`, never otherwise
- D) Yes, owners always can

<details><summary>Answer</summary>

**B.** Only the owner class applies, although `ann` could change the mode.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select all true statements about a directory with mode `2775`

- [ ] A) New files get its group
- [ ] B) New subdirectories inherit setgid
- [ ] C) Only owners can delete their files
- [ ] D) Others can create files in it

<details><summary>Answer</summary>

**A, B.**

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Find the problem

```bash
chmod -R 644 /var/www/site
```

<details><summary>Answer</summary>

Directories lose `x` and become impassable. Use `find` separately for directories (`755`) and files (`644`).

→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 What remains of mode `6755` after root runs `chown ann` on a regular file?

- A) `6755`
- B) `755`
- C) `4755`
- D) `000`

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 A file has an ACL `user:bob:r`. After `chmod g-r`

- A) bob still reads it
- B) the mask drops `r`, bob cannot read it
- C) the ACL is deleted
- D) nothing changes

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 `find -perm /6000` finds files with

- A) exactly mode `6000`
- B) setuid or setgid
- C) both setuid and setgid
- D) neither

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: a web server answers "permission denied" for a file with mode `644`. What do you check?

<details><summary>Key points</summary>

- Which user the server runs as (`ps`, `id`).
- `x` on every directory of the path (`namei -l`).
- ACLs, attributes, mount options.
- AppArmor or SELinux messages in the logs.

→ [Expert](expert.en.md)

</details>
