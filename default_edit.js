const fs = require('fs');
const obj = JSON.parse(fs.readFileSync('default.json', 'utf8'));

obj.boards.forEach((b, bi) => {
    b.columns.forEach((c, ci) => {
        c.order = ci;
        c.tasks.forEach((m, mi) => {
            m.order = mi;
            m.subtasks.forEach((s, si) => {
                s.order = si;
            });
        });
    });
});

fs.writeFileSync('default2.json', JSON.stringify(obj), 'utf8');
