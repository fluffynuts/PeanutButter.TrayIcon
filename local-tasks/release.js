const
  gulp = require("gulp"),
  { rm } = require("yafs"),
  { packageDir } = require("./config"),
  runSequence = requireModule("run-sequence");

gulp.task("build-binaries-for-nuget-packages", done => {
  process.env.BUILD_CONFIGURATION = "Release";
  return runSequence("build", done);
});

gulp.task("prepack", ["build-binaries-for-nuget-packages"], () => Promise.resolve());


gulp.task("clean-release", async () => {
  await rm(packageDir);
});

gulp.task("release", done => {
  console.log("run release");
  return runSequence("clean-release", "pack", "push", "commit-release", "tag-and-push", done);
});