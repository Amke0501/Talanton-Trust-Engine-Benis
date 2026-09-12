'use client'

import { useState, type ReactNode } from 'react'
import { X, ShieldCheck, UserCheck, KeyRound } from 'lucide-react'

/**
 * What happens when someone wants an account.
 *
 * The landing page used to offer a "Sign up" button wired to nothing. It could not honestly be
 * wired to a self-registration form either: SACCO membership is vetted offline, and the account
 * someone holds decides whether they can approve loans or release money. A stranger who signed
 * themselves up would receive a working password and then be refused at every turn, which is a
 * worse experience than being told plainly how membership actually works.
 *
 * So this explains the real route rather than pretending there is a shortcut.
 */
const STEPS = [
  {
    icon: UserCheck,
    title: 'Your SACCO admits you as a member',
    body: 'Membership is arranged with the cooperative directly, the same way it always has been. Talanton does not decide who joins.',
  },
  {
    icon: KeyRound,
    title: 'They register you and create your sign-in',
    body: 'A member of staff enters your name and email, and the system creates your account with an initial password they pass to you.',
  },
  {
    icon: ShieldCheck,
    title: 'You sign in and change your password',
    body: 'Your account already knows which part of the service you use, so there is nothing to choose or configure.',
  },
]

export function RequestAccessModal({
  children,
  className,
}: {
  children: ReactNode
  className?: string
}) {
  const [open, setOpen] = useState(false)

  return (
    <>
      <button type="button" className={className} onClick={() => setOpen(true)}>
        {children}
      </button>

      {open && (
        <div
          role="dialog"
          aria-modal="true"
          aria-labelledby="request-access-title"
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm"
          onClick={(e) => {
            if (e.target === e.currentTarget) setOpen(false)
          }}
        >
          <div className="w-full max-w-lg rounded-3xl bg-white p-7 shadow-2xl">
            <div className="flex items-start justify-between gap-4">
              <div>
                <p className="text-[0.65rem] font-bold uppercase tracking-widest text-[#6f8f1f]">
                  Getting an account
                </p>
                <h2
                  id="request-access-title"
                  className="mt-1.5 font-serif text-2xl font-bold text-[#103a27]"
                >
                  Accounts come from your SACCO
                </h2>
              </div>
              <button
                type="button"
                onClick={() => setOpen(false)}
                aria-label="Close"
                className="rounded-full p-1.5 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-700"
              >
                <X className="size-5" />
              </button>
            </div>

            <p className="mt-3 text-sm leading-relaxed text-gray-600">
              There is no public sign-up, and that is deliberate. Your account determines whether you
              can approve a loan or release funds, so it is created by the cooperative rather than
              claimed by whoever asks.
            </p>

            <ol className="mt-5 space-y-4">
              {STEPS.map((step, index) => (
                <li key={step.title} className="flex gap-3.5">
                  <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-[#f2f7e4] text-[#3f5a12]">
                    <step.icon className="size-4" />
                  </span>
                  <div>
                    <p className="text-sm font-bold text-[#103a27]">
                      <span className="mr-1.5 font-mono text-xs text-[#6f8f1f]">
                        {String(index + 1).padStart(2, '0')}
                      </span>
                      {step.title}
                    </p>
                    <p className="mt-0.5 text-xs leading-relaxed text-gray-600">{step.body}</p>
                  </div>
                </li>
              ))}
            </ol>

            <div className="mt-6 rounded-2xl bg-[#f4f5f4] px-4 py-3.5">
              <p className="text-xs leading-relaxed text-gray-600">
                <strong className="text-[#103a27]">Already a member?</strong> Speak to your SACCO
                office and they will set you up. If you have your details, use{' '}
                <strong className="text-[#103a27]">Log in</strong> instead.
              </p>
            </div>

            <div className="mt-5 flex justify-end">
              <button
                type="button"
                onClick={() => setOpen(false)}
                className="rounded-full bg-[#103a27] px-5 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-[#124a31]"
              >
                Got it
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  )
}
