/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    fs = require("fs"),
    jshint = require("gulp-jshint"),
    //less = require("gulp-less"),
    project = require('./project.json'),
    //sass = require("gulp-sass"),
    uglify = require("gulp-uglify");

var paths = {
    styles: "Styles",
    webroot: "./wwwroot/"
};

paths.js = paths.webroot + "js/**/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.css = paths.webroot + "css/**/*.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatCssDest = paths.webroot + "css/site.min.css";

gulp.task("clean:js", function (cb) {
    rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

/*
gulp.task("less", function () {
    return gulp.src(paths.styles + '/main.less')
        .pipe(less())
        .pipe(gulp.dest(project.webroot + '/css'));
});

gulp.task("sass", function () {
    return gulp.src(paths.styles + '/main.scss')
        .pipe(sass())
        .pipe(gulp.dest(project.webroot + '/css'));
});
*/

gulp.task("lint", function () {
    return gulp.src(paths.js)
       .pipe(jshint())
       .pipe(jshint.reporter("jshint-stylish"))
       .pipe(jshint.reporter("fail"));
});

gulp.task("min:js", function () {
    return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:css", function () {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("min", ["min:js", "min:css"]);

gulp.task("build", ["lint", "clean", "min"/*, "less", "saas"*/]);
