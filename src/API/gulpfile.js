﻿/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    csslint = require("gulp-csslint"),
    jasmine = require("gulp-jasmine"),
    jshint = require("gulp-jshint"),
    less = require("gulp-less"),
    lesshint = require("gulp-lesshint"),
    rename = require("gulp-rename"),
    sass = require("gulp-sass"),
    sassLint = require("gulp-sass-lint");

var webroot = "./wwwroot/";
var styles = "./Assets/Styles";

var paths = {
    js: webroot + "js/**/*.js",
    minJs: webroot + "js/**/*.min.js",
    testsJs: "js/**/*.spec.js",
    css: webroot + "css/**/*.css",
    minCss: webroot + "css/**/*.min.css",
    concatJsDest: webroot + "js/site.min.js",
    concatCssDest: webroot + "css/site.min.css",
    less: styles + "/less/site.less",
    lessDest: webroot + "css",
    sass: styles + "/sass/site.scss",
    sassDest: webroot + "css"
};

gulp.task("clean:js", function (cb) {
    rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("css:less", function () {
    return gulp.src(paths.less)
      .pipe(less())
      .pipe(gulp.dest(paths.lessDest));
});

gulp.task("css:sass", function () {
    return gulp.src(paths.sass)
      .pipe(sass().on("error", sass.logError))
      .pipe(rename("sass.css"))
      .pipe(gulp.dest(paths.sassDest));
});

gulp.task("css", ["css:less", "css:sass"]);

gulp.task("lint:css", function () {
    return gulp.src(styles)
      .pipe(csslint())
      .pipe(csslint.reporter())
      .pipe(csslint.reporter("fail"));
});

gulp.task("lint:js", function () {
    return gulp.src(paths.js)
      .pipe(jshint())
      .pipe(jshint.reporter("default"))
      .pipe(jshint.reporter("fail"));
});

gulp.task("lint:less", function () {
    return gulp.src(paths.less)
        .pipe(lesshint())
        .pipe(lesshint.reporter());
});

gulp.task("lint:sass", function () {
    gulp.src(paths.sass)
      .pipe(sassLint())
      .pipe(sassLint.format())
      .pipe(sassLint.failOnError())
});

gulp.task("lint", ["lint:js", "lint:less", "lint:sass", "lint:css"]);

gulp.task("min:js", function () {
    return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:css", ["css"], function () {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("min", ["min:js", "min:css"]);

gulp.task("test:js", function () {
    return gulp.src(paths.testsJs)
      .pipe(jasmine());
});

gulp.task("test", ["test:js"]);

gulp.task("build", ["lint", "min"]);
gulp.task("publish", ["build", "test"]);
