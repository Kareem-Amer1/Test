## Project Overview

I want you to build a Quiz System designed specifically for evaluating
job candidates inside a company. The core idea is that each job Position
has its own exam Template, and the exam is conducted on the HR's device
during the interview itself — meaning the candidate sits in front of the
computer and answers the questions while the interview is taking place.

The system aims to provide:
- A unified and fair evaluation for all candidates applying for
  the same Position
- Time savings for HR through automatic grading of objective questions
- A permanent archive of all exams that can never be deleted

The system has two levels of users:
The Super Admin who manages the entire system,
and the HR who uses the system to conduct exams and evaluate candidates.

---

## Users & Permissions (Detailed)

### Super Admin
- The primary administrator of the entire system
- Inherits ALL permissions that a regular HR has, in addition
  to their own exclusive permissions
  (everything an HR can do, the Super Admin can do as well,
  but NOT the other way around)
- Can add new HR accounts and delete existing ones
- Can add new Positions (job specializations) and delete existing ones
  (edge cases around deleting a Position must be handled carefully)
- Can view all exams created by all HRs without exception
- Can view the full details and results of any exam

### HR
- Logs into the system using their own personal account
- Can modify the Template of any existing Position in the system
  (this change affects the shared Template across all HRs)
- Can create a new exam by:
  - Selecting the Position
  - Entering only the candidate's name (no other data needed)
- Can only view their own exams — cannot see any other HR's exams
- After an exam ends, can review the candidate's answers
- Can manually grade essay questions and assign scores

---

## Positions & Templates (Detailed)

### Template Concept
Each Position has one shared Template used by all HRs.
This Template defines the structure and content of the exam
for that specific Position.

### The Template Contains:

1. Exam Duration (Timer):
   - Unified across all exams for this Position
   - If an HR changes the duration, all future exams will use
     the new duration
   - Past/completed exams are NOT affected by this change

2. Questions and their types:

   a. Essay (Open-ended):
      - Candidate writes a free-text answer
      - NOT auto-graded
      - HR manually assigns a score after the exam

   b. True / False:
      - Candidate selects True or False
      - Auto-graded
      - HR defines the correct answer when adding the question

   c. Multiple Choice (MCQ):
      - HR defines the number of answer choices
      - HR writes the text for each choice
      - HR marks the correct answer
      - Auto-graded

3. Partitions (visual organization):
   - Questions inside a Template are grouped into Partitions
   - Each Partition has a name (e.g. "Soft Skills", "AI", ".NET") and
     contains related questions
   - Partitions are NOT fixed — HR or Super Admin creates and names them
     and decides which questions belong to each one
   - Partitions are stored in the database (not front-end only)
   - When adding a question, the user selects which Partition it belongs to
   - Deleting a Partition cascade-deletes all questions inside it
   - During the exam, questions are displayed under their Partition header;
     the Partition name appears with every question
   - No scoring or grading logic is tied to Partitions — organization only

### Data Seeding:
- The system starts with predefined Positions such as:
  (Software Engineer, Sales, IT Support, and others)
- Each Position comes with predefined questions and a time limit
- This saves the company from starting from scratch

### Modifying the Template:
- HR can modify the Template by:
  - Changing the exam duration
  - Creating, renaming, or deleting Partitions (delete removes all
    questions in that Partition)
  - Adding a new question to a chosen Partition (specifying its type
    and all details)
  - Deleting or reordering questions within a Partition
- Any modification affects all HRs since the Template is shared

---

## User Profile (Any authenticated user)

- View account information: email, role, member since
- Update own full name
- Change own password (requires current password)
- Email and role are read-only on the profile screen

---

## Exam Flow (Detailed)

### Creating an Exam:
- HR selects the Position
- HR enters the candidate's name
- The system loads the Template for that Position
  (questions and timer are both pulled from the Template)
- The exam begins on the HR's device

### During the Exam:
- The candidate answers the questions on the HR's device
- Questions are grouped under Partition headers; each question shows
  its Partition name
- The exam session runs full-screen without app navigation (sidebar,
  back links) — the candidate cannot leave until the exam is submitted
  (manual submit or timer auto-submit)
- A countdown timer is displayed
- When time runs out, the exam is automatically submitted
- The candidate can also manually submit before time runs out

### After the Exam:
- MCQ and True/False questions are automatically graded
  and the score is calculated instantly
- HR opens the exam to review the essay answers
- HR manually assigns a score to each essay question
- The system calculates the total overall score

---

## Business Rules

- Exams can NEVER be deleted from the system under any circumstances
- HR can only view their own exams, not those of other HRs
- Super Admin can view all exams across all HRs
- Super Admin inherits ALL HR permissions — but HR does NOT
  inherit Super Admin permissions
- Modifying the Template only affects future exams,
  completed exams remain unchanged
- Each exam must store a snapshot of the questions at the time
  it was taken, so that changes to the Template do not alter
  historical exam records

---

## What I Need From You Before Writing Any Code:

1. Read all the details carefully and confirm you understand the concept
2. Create a full project plan that includes:
   - Detailed Database Schema (tables and relationships)
   - Project Architecture
   - Complete list of Features
   - Required API Endpoints
3. Suggest the most suitable Tech Stack with justification
   for each technology choice
4. Break the project into clear and ordered Phases
5. Wait for my approval on the Plan before starting implementation