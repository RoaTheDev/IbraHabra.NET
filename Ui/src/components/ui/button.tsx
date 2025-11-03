import * as React from "react"
import { Slot } from "@radix-ui/react-slot"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const buttonVariants = cva(
  // Base
  "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium transition-all duration-200 select-none " +
  "disabled:pointer-events-none disabled:opacity-50 [&_svg]:pointer-events-none [&_svg:not([class*='size-'])]:size-4 shrink-0 " +
  "outline-none focus-visible:ring-[3px] focus-visible:ring-ring/50 focus-visible:ring-offset-0 focus-visible:outline-none " +
  "aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40 aria-invalid:border-destructive",

  {
    variants: {
      variant: {
        default:
          "bg-[var(--color-primary)] text-[var(--color-primary-foreground)] " +
          "hover:bg-[var(--color-primary-hover)] hover:shadow-[0_0_15px_var(--color-primary)] " +
          "focus-visible:ring-[var(--color-primary)] active:scale-[0.98]",

        destructive:
          "bg-[var(--color-destructive)] text-[var(--color-destructive-foreground)] " +
          "hover:bg-[oklch(0.65_0.22_25)] hover:shadow-[0_0_15px_var(--color-destructive)] " +
          "focus-visible:ring-[var(--color-destructive)] active:scale-[0.98]",

        outline:
          "border border-[var(--color-border)] bg-[var(--color-background)] text-[var(--color-foreground)] " +
          "hover:bg-[var(--color-border)] hover:text-white hover:shadow-[0_0_20px_var(--color-accent)] hover:border-[var(--color-accent)] " +
          "focus-visible:ring-[var(--color-accent)] active:scale-[0.98]",

        secondary:
          "bg-[var(--color-secondary)] text-[var(--color-secondary-foreground)] " +
          "hover:bg-[color-mix(in_oklch,var(--color-secondary)_80%,white)] hover:shadow-[0_0_10px_var(--color-secondary)] " +
          "focus-visible:ring-[var(--color-secondary)] active:scale-[0.98]",

        ghost:
          "text-[var(--color-foreground)] hover:bg-[var(--color-accent)]/10 hover:text-[var(--color-accent)] " +
          "focus-visible:ring-[var(--color-accent)] active:scale-[0.98]",

        link:
          "text-[var(--color-primary)] underline-offset-4 hover:underline hover:text-[var(--color-primary-hover)]",
      },

      size: {
        default: "h-9 px-4 py-2 has-[>svg]:px-3",
        sm: "h-8 rounded-md gap-1.5 px-3 has-[>svg]:px-2.5 text-xs",
        lg: "h-10 rounded-md px-6 has-[>svg]:px-4 text-base",
        icon: "size-9",
        "icon-sm": "size-8",
        "icon-lg": "size-10",
      },
    },

    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
)

function Button({
                  className,
                  variant,
                  size,
                  asChild = false,
                  ...props
                }: React.ComponentProps<"button"> &
  VariantProps<typeof buttonVariants> & { asChild?: boolean }) {
  const Comp = asChild ? Slot : "button"

  return (
    <Comp
      data-slot="button"
      className={cn(buttonVariants({ variant, size, className }))}
      {...props}
    />
  )
}

export { Button, buttonVariants }
