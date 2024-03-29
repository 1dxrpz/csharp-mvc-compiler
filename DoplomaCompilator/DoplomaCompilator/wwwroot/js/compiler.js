var CodeEditor;
$(document).ready(() => {

    var nums = "0123456789", space = "          ";
    var colors = ["#fcc", "#f5f577", "#cfc", "#aff", "#ccf", "#fcf"];
    var rulers = [], value = "";
    for (var i = 1; i <= 1; i++) {
        rulers.push({ color: colors[i], column: i * 10, lineStyle: "dashed" });
        for (var j = 1; j < i; j++) value += space;
        value += nums + "\n";
    }

    CodeMirror.commands.autocomplete = function (cm) {
        cm.showHint();
    }

    CodeEditor = CodeMirror.fromTextArea(document.querySelector("#code_editor"), {
        value: "function myScript(){return 100;}\n",
        mode: "text/x-csrc",
        theme: "shadowfox",
        lineNumbers: true,
        styleActiveLine: true,
        rulers: rulers,
        indentUnit: 4,
        tabSize: 4,
        autoCloseBrackets: true,
        lineWrapping: true,
        smartIndent: true,
        matchBrackets: true,
        indentWithTabs: true,
    });
});

function ClearResult() {
    document.querySelector(`.result[data-stage='build']`).innerHTML = "";
    document.querySelector(`.result[data-stage='run']`).innerHTML = "";
    document.querySelector(`.result[data-stage='tests']`).innerHTML = "";
}
function SetResult(stage, message, elapsed) {
    document.querySelector(`.result[data-stage='${stage}']`).innerHTML =
        `<div class="message">${message}</div>
                                        <div class="elapsed">Elapsed time: ${elapsed}</div>`;
}
var buildStage = document.querySelector(".compilation_stage[data-stage='build']");
var runStage = document.querySelector(".compilation_stage[data-stage='run']");
var testsStage = document.querySelector(".compilation_stage[data-stage='tests']");

async function Compile() {
    ClearResult();

    buildStage.setAttribute("data-status", "wait");
    runStage.setAttribute("data-status", "default");
    testsStage.setAttribute("data-status", "default");


    var build_result = JSON.parse(await $.ajax({
        url: '/Home/CompileCode',
        method: 'post',
        dataType: 'text',
        data: { code: CodeEditor.getValue() },
        success: function (data) {
        }
    }));

    if (!build_result.buildSucceed) {
        let errors = build_result.errors
            .map(v => `<div class="error"><div class="line">${v.line}</div>${v.errorMessage}</div><br>`)
            .reduce((a, b) => `${a}${b}`);

        buildStage.setAttribute("data-status", "fail");

        SetResult("build", `${errors}`, build_result.elapsedTime);
        return;
    } else {
        SetResult("build", ``, build_result.elapsedTime);
    }
    buildStage.setAttribute("data-status", "success");
    runStage.setAttribute("data-status", "wait");

    var run_result = JSON.parse(await $.ajax({
        url: '/Home/RunCode',
        method: 'post',
        dataType: 'text',
        success: function (data) {
        }
    }));

    if (!run_result.buildSucceed) {
        let errors = run_result.errors
            .map(v => `<div class="error"><div class="line">${v.line}</div>${v.errorMessage}</div><br>`)
            .reduce((a, b) => `${a}${b}`);

        runStage.setAttribute("data-status", "fail");

        SetResult("run", `${errors}`, run_result.elapsedTime);
        return;
    } else {
        SetResult("run", `${run_result.result}`, run_result.elapsedTime);
    }

    runStage.setAttribute("data-status", "success");
    testsStage.setAttribute("data-status", "wait");

    var tests_result = JSON.parse(await $.ajax({
        url: '/Home/RunTests',
        method: 'post',
        dataType: 'text',
        success: function (data) {
        }
    }));

    if (tests_result.passed != tests_result.total) {
        testsStage.setAttribute("data-status", "fail");
        SetResult("tests", `Passed ${tests_result.passed} of ${tests_result.total} test`, run_result.elapsedTime);
    } else {
        SetResult("tests", `All tests passed`, tests_result.elapsedTime);
        testsStage.setAttribute("data-status", "success");
    }
}