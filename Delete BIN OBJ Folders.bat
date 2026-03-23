@echo off
@echo Deleting all BIN, OBJ folders...
ECHO.

FOR /d /r . %%d in (bin,obj) DO (
	IF EXIST "%%d" (
		ECHO %%d | FIND /I "\node_modules\" > Nul && (
			ECHO.Skipping: %%d
		) || (
			ECHO.Deleting: %%d
			rd /s/q "%%d"
		)
	)
)

@echo.
@echo BIN and OBJ folders successfully deleted :) Close the window.
@echo.
@echo.