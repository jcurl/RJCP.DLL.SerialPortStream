#
# Check for specific features of the platform being compiles.
#

include(CheckSymbolExists)
check_symbol_exists(min "stdlib.h" HAVE_STDLIB_MIN)
